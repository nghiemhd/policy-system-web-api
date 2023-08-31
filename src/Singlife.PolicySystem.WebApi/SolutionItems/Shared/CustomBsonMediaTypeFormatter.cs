using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.IO;

using System.Text;

namespace SingLife.PolicySystem.Shared.Formatter
{
    internal class CustomBsonMediaTypeFormatter : BsonMediaTypeFormatter
    {
        public override JsonReader CreateJsonReader(Type type, Stream readStream, Encoding effectiveEncoding)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (readStream == null)
                throw new ArgumentNullException(nameof(readStream));

            if (effectiveEncoding == null)
                throw new ArgumentNullException(nameof(effectiveEncoding));

            var reader = new BsonDataReader(new BinaryReader(readStream, effectiveEncoding))
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateTimeKindHandling = DateTimeKind.Utc
            };

            try
            {
                reader.ReadRootValueAsArray =
                    typeof(IEnumerable).IsAssignableFrom(type) && !typeof(IDictionary).IsAssignableFrom(type);
            }
            catch when (Dispose(reader))
            {
                // This catch block will never be reached.
            }

            return reader;
        }

        private bool Dispose(IDisposable disposable)
        {
            // The exception filter always returns false to preserve the call stack.
            // https://thomaslevesque.com/2015/06/21/exception-filters-in-c-6/
            disposable.Dispose();
            return false;
        }
    }
}