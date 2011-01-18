using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace Squiggle.Chat.Services.Chat
{
    [DataContract]
    public class ChatEndPoint
    {
        [DataMember]
        public string ClientID { get; set; }
        [DataMember]
        public IPEndPoint Address { get; set; }

        public ChatEndPoint()
        {

        }

        public ChatEndPoint(string id, IPEndPoint address)
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
            if (obj is ChatEndPoint)
            {
                var other = (ChatEndPoint)obj;
                return this.ClientID.Equals(other.ClientID) &&
                        this.Address.Equals(other.Address);
            }
            return false;
        }
    }
}
