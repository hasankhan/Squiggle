using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Squiggle.Utilities.Serialization
{
    public class IPEndPointJsonConverter : JsonConverter<IPEndPoint>
    {
        public override IPEndPoint? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s))
                return null;
            return IPEndPoint.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
