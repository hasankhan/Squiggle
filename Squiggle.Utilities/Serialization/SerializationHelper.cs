using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Squiggle.Utilities.Serialization
{
    public class SerializationHelper
    {
        static SerializationHelper()
        {
            // In protobuf-net v3, AllowParseableTypes was removed.
            // Types like IPAddress are now handled via string-backed properties
            // on the serialized classes (see SquiggleEndPoint).
        }

        [RequiresUnreferencedCode("protobuf-net Serializer.Serialize uses reflection-based serialization")]
        public static byte[] Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(stream, item);
            return stream.ToArray();
        }        

        [RequiresUnreferencedCode("protobuf-net Serializer.Deserialize uses reflection-based serialization")]
        public static void Deserialize<T>(byte[] data, Action<T> onDeserialize, string entityName) where T:class
        {
            T? obj = null;
            if (ExceptionMonster.EatTheException(() =>
            {
                obj = SerializationHelper.Deserialize<T>(data);
            }, "deserializing " + entityName + " of type " + typeof(T).Name))
            {
                onDeserialize(obj!);
            }
        }

        [RequiresUnreferencedCode("protobuf-net Serializer.Deserialize uses reflection-based serialization")]
        static T Deserialize<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var stream = new MemoryStream(data);
            T item = ProtoBuf.Serializer.Deserialize<T>(stream);
            return item;
        }
    }
}
