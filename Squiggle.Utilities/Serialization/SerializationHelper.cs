using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProtoBuf.Meta;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Squiggle.Utilities.Serialization
{
    public class SerializationHelper
    {
        static SerializationHelper()
        {
            RuntimeTypeModel.Default.AllowParseableTypes = true;
        }

        public static byte[] Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(stream, item);
            return stream.ToArray();
        }        

        public static void Deserialize<T>(byte[] data, Action<T> onDeserialize, string entityName) where T:class
        {
            T obj = null;
            if (ExceptionMonster.EatTheException(() =>
            {
                obj = SerializationHelper.Deserialize<T>(data);
            }, "deserializing " + entityName + " of type " + typeof(T).Name))
            {
                onDeserialize(obj);
            }
        }

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
