using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters
{
    public class BsonSerializerObjectPolicy : IPooledObjectPolicy<JsonSerializer>
    {
        private readonly JsonSerializerSettings serializerSettings;

        public BsonSerializerObjectPolicy(JsonSerializerSettings serializerSettings)
        {
            this.serializerSettings = serializerSettings;
        }

        public JsonSerializer Create() => JsonSerializer.Create(serializerSettings);

        public bool Return(JsonSerializer serializer) => true;
    }
}
