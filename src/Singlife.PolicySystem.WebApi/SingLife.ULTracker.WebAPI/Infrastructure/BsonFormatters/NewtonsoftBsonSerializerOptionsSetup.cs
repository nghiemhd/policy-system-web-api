using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters
{
    public class NewtonsoftBsonSerializerOptionsSetup : ConfigureOptions<MvcOptions>
    {
        public NewtonsoftBsonSerializerOptionsSetup()
            : base(ConfigureMvc)
        {
        }

        private static void ConfigureMvc(MvcOptions options)
        {
            options.InputFormatters.Add(CreateBsonInputFormatter(options));
            options.OutputFormatters.Add(CreateBsonOutputFormatter(options));
        }

        private static BsonInputFormatter CreateBsonInputFormatter(MvcOptions options) =>
            new BsonInputFormatter(CreateJsonSerializerSettings(), new DefaultObjectPoolProvider(), options);

        private static BsonOutputFormatter CreateBsonOutputFormatter(MvcOptions options) =>
            new BsonOutputFormatter(CreateJsonSerializerSettings(), options);

        private static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new DefaultContractResolver
                {
                    IgnoreSerializableAttribute = true
                }
            };
        }
    }
}