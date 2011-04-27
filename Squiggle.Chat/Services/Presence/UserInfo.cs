using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;

namespace Squiggle.Chat.Services.Presence
{
    [Serializable, DataContract]
    public class UserInfo
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string EmailAddress { get; set; }
        [DataMember]
        public IPEndPoint ChatEndPoint { get; set; }
        [DataMember]
        public IPEndPoint PresenceEndPoint { get; set; }
        [DataMember]
        public TimeSpan KeepAliveSyncTime { get; set; }
        [DataMember]
        public UserStatus Status { get; set; }
        [DataMember]
        public Dictionary<string, string> Properties { get; set;  }

        public void Update(UserInfo user)
        {
            this.DisplayName = user.DisplayName;
            this.Properties = user.Properties;
            this.Status = user.Status;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is UserInfo)
            {
                string id = ((UserInfo)obj).ID;
                return ID.Equals(id);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (this.ID != null)
                return ID.GetHashCode();
            return base.GetHashCode();
        }
    }
}
