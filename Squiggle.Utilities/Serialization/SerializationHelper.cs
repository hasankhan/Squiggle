using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Squiggle.Utilities.Serialization
{
    public class SerializationHelper
    {
        public static byte[] Serialize<T>(T item)
        {
            var stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(stream, item);
            return stream.ToArray();
        }

        public static T Deserialize<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var stream = new MemoryStream(data);
            T item = ProtoBuf.Serializer.Deserialize<T>(stream);
            return item;
        }
    }
}
