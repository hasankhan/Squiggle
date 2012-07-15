using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Squiggle.Core
{
    [DataContract]
    public class SquiggleEndPoint
    {
        [ProtoMember(1)]
        [DataMember]
        public string ClientID { get; set; }
        [ProtoMember(2)]
        IPAddress IP { get; set; }
        [ProtoMember(3)]
        int Port { get; set; }

        [DataMember]
        public IPEndPoint Address
        {
            get { return new IPEndPoint(IP, Port); }
            set
            {
                IP = value.Address;
                Port = value.Port;
            }
        }

        public SquiggleEndPoint() { }

        public SquiggleEndPoint(string id, IPEndPoint address)
        {
            if (String.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            this.ClientID = id;
            this.Address = address;
        }

        public override int GetHashCode()
        {
            int x = String.IsNullOrEmpty(ClientID) ? 0 : ClientID.GetHashCode();
            int y = Address == null ? 0 : Address.GetHashCode();

            int hash = 17;
            hash = hash * 31 + x;
            hash = hash * 31 + y;
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is SquiggleEndPoint)
            {
                var other = (SquiggleEndPoint)obj;
                return this.ClientID.Equals(other.ClientID) &&
                        this.Address.Equals(other.Address);
            }
            return false;
        }

        public override string ToString()
        {
            string output = ClientID + "@" + Address;
            return output;
        }
    }
}
