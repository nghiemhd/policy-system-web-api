using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters
{
    /// <summary>
    /// A BSON input formatter that uses <see cref="BsonDataReader"/> to deserialize BSON data
    /// to strongly typed objects. Please read the remarks for more details.
    /// </summary>
    /// <remarks>
    /// The <see cref="BsonDataReader"/> class uses synchronous IO APIs, which have been disabled by default
    /// since ASP.NET Core 3.0 for good reasons
    /// (https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnetcore#http-synchronous-io-disabled-in-all-servers).
    /// This class uses the technique described here (https://github.com/dotnet/aspnetcore/issues/7644#issuecomment-493632670)
    /// and here (https://github.com/dotnet/aspnetcore/blob/093df67c06297c20edb422fe6d3a555008e152a9/src/Mvc/Mvc.Formatters.Xml/src/XmlSerializerInputFormatter.cs#L102-L117)
    /// to make the synchronous <see cref="BsonDataReader"/> class compatible with the new ASP.NET Core.
    /// </remarks>
    public class BsonInputFormatter : TextInputFormatter
    {
        private const int DefaultMemoryThreshold = 1024 * 30;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly ObjectPoolProvider objectPoolProvider;
        private readonly MvcOptions mvcOptions;
        private ObjectPool<JsonSerializer> jsonSerializerPool;

        public BsonInputFormatter(
            JsonSerializerSettings jsonSerializerSettings,
            ObjectPoolProvider objectPoolProvider,
            MvcOptions mvcOptions)
        {
            this.jsonSerializerSettings = jsonSerializerSettings;
            this.objectPoolProvider = objectPoolProvider;
            this.mvcOptions = mvcOptions;

            SupportedEncodings.Add(Encoding.Unicode);
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/bson"));
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var request = context.HttpContext.Request;
            Stream readStream = new NonDisposableStream(request.Body);

            if (!request.Body.CanSeek && !mvcOptions.SuppressInputFormatterBuffering)
            {
                // XmlSerializer does synchronous reads. In order to avoid blocking on the stream, we asynchronously
                // read everything into a buffer, and then seek back to the beginning.
                var memoryThreshold = DefaultMemoryThreshold;
                if (request.ContentLength.HasValue && request.ContentLength.Value > 0 && request.ContentLength.Value < memoryThreshold)
                {
                    // If the Content-Length is known and is smaller than the default buffer size, use it.
                    memoryThreshold = (int)request.ContentLength.Value;
                }

                readStream = new FileBufferingReadStream(request.Body, memoryThreshold);

                await readStream.DrainAsync(CancellationToken.None);
                readStream.Seek(0L, SeekOrigin.Begin);
            }

            using var bsonReader = new BsonDataReader(readStream)
            {
                ReadRootValueAsArray = IsEnumerable(context.ModelType),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateTimeKindHandling = DateTimeKind.Utc
            };

            var successful = true;
            EventHandler<ErrorEventArgs> errorHandler = (sender, eventArgs) =>
            {
                successful = false;
                eventArgs.ErrorContext.Handled = true;
            };

            var jsonSerializer = CreateJsonSerializer();
            jsonSerializer.Error += errorHandler;
            var type = context.ModelType;
            object model;

            try
            {
                model = jsonSerializer.Deserialize(bsonReader, type);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }

            return successful
                ? await InputFormatterResult.SuccessAsync(model)
                : await InputFormatterResult.FailureAsync();
        }

        private JsonSerializer CreateJsonSerializer()
        {
            jsonSerializerPool ??= objectPoolProvider.Create(new BsonSerializerObjectPolicy(jsonSerializerSettings));

            return jsonSerializerPool.Get();
        }

        private static bool IsEnumerable(Type type) => typeof(IEnumerable).IsAssignableFrom(type);
    }
}