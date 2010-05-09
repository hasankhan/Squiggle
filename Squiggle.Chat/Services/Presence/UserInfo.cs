using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Squiggle.Chat.Services.Presence
{
    [Serializable]
    internal sealed class UserInfo
    {
        public string UserFriendlyName { get; set; }
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public int KeepAliveSyncTime { get; set; }

        public IPEndPoint ChatEndPoint
        {
            get { return new IPEndPoint(Address, Port); }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is UserInfo)
            {
                IPAddress address = ((UserInfo)obj).Address;
                if (this.Address != null && address != null)
                {
                    return this.Address.Equals(address);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (this.Address != null)
            {
                return this.Address.GetHashCode();
            }
            return base.GetHashCode();
        }

        public static void Serialize(Stream writer, UserInfo graph)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (graph == null)
            {
                throw new ArgumentNullException("graph");
            }

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(writer, graph);
        }

        public static UserInfo Deserialize(Stream reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("writer");
            }

            BinaryFormatter formatter = new BinaryFormatter();
            UserInfo data = (UserInfo)formatter.Deserialize(reader);

            return data;
        }
    }
}
