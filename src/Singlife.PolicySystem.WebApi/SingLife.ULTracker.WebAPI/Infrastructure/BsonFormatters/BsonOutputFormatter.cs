using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters
{
    /// <summary>
    /// A BSON output formatter that uses <see cref="BsonDataWriter"/> to serialize objects to BSON data.
    /// Please read the remarks for more details.
    /// </summary>
    /// <remarks>
    /// The <see cref="BsonDataWriter"/> class uses synchronous IO APIs, which have been disabled by default
    /// since ASP.NET Core 3.0 for good reasons
    /// (https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnetcore#http-synchronous-io-disabled-in-all-servers).
    /// This class uses the technique described here (https://github.com/dotnet/aspnetcore/issues/7644#issuecomment-493632670)
    /// and here (https://github.com/dotnet/aspnetcore/blob/093df67c06297c20edb422fe6d3a555008e152a9/src/Mvc/Mvc.Formatters.Xml/src/XmlSerializerOutputFormatter.cs#L243-L273)
    /// to make the synchronous <see cref="BsonDataWriter"/> class compatible with the new ASP.NET Core.
    /// </remarks>
    public class BsonOutputFormatter : TextOutputFormatter
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly MvcOptions mvcOptions;
        private JsonSerializer jsonSerializer;

        public BsonOutputFormatter(JsonSerializerSettings serializerSettings, MvcOptions mvcOptions)
        {
            jsonSerializerSettings = serializerSettings ?? throw new ArgumentNullException(nameof(serializerSettings));
            this.mvcOptions = mvcOptions;

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/bson"));
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (selectedEncoding == null)
                throw new ArgumentNullException(nameof(selectedEncoding));

            var response = context.HttpContext.Response;
            var responseStream = response.Body;
            FileBufferingWriteStream fileBufferingWriteStream = null;

            if (!mvcOptions.SuppressOutputFormatterBuffering)
            {
                fileBufferingWriteStream = new FileBufferingWriteStream();
                responseStream = fileBufferingWriteStream;
            }

            try
            {
                using var bsonWriter = new BsonDataWriter(responseStream)
                {
                    CloseOutput = false
                };

                CreateJsonSerializer()
                    .Serialize(bsonWriter, context.Object);

                await bsonWriter.FlushAsync();

                if (fileBufferingWriteStream != null)
                {
                    response.ContentLength = fileBufferingWriteStream.Length;
                    await fileBufferingWriteStream.DrainBufferAsync(response.Body);
                }
            }
            finally
            {
                if (fileBufferingWriteStream != null)
                {
                    await fileBufferingWriteStream.DisposeAsync();
                }
            }
        }

        private JsonSerializer CreateJsonSerializer() => jsonSerializer ??= JsonSerializer.Create(jsonSerializerSettings);
    }
}
