using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Squiggle.Utilities.Serialization
{
    public class SerializationHelper
    {
        static readonly JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new IPEndPointJsonConverter() }
        };

        public static byte[] Serialize<T>(T item)
        {
            return JsonSerializer.SerializeToUtf8Bytes(item, options);
        }

        public static void Deserialize<T>(byte[] data, Action<T> onDeserialize, string entityName) where T : class
        {
            T? obj = null;
            if (ExceptionMonster.EatTheException(() =>
            {
                obj = JsonSerializer.Deserialize<T>(data, options);
            }, "deserializing " + entityName + " of type " + typeof(T).Name))
            {
                if (obj != null)
                    onDeserialize(obj);
            }
        }
    }
}
