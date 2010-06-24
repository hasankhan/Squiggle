using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Squiggle.Chat.Services.Presence
{
    [Serializable, DataContract]
    public class UserInfo
    {
        [DataMember]
        public string UserFriendlyName { get; set; }
        [DataMember]
        public IPEndPoint ChatEndPoint { get; set; }
        [DataMember]
        public IPEndPoint PresenceEndPoint { get; set; }
        [DataMember]
        public TimeSpan KeepAliveSyncTime { get; set; }
        [DataMember]
        public string DisplayMessage { get; set; }
        [DataMember]
        public UserStatus Status { get; set; }
        [DataMember]
        public Dictionary<string, string> Properties { get; set;  }

        public static UserInfo FromEndPoint(IPEndPoint endPoint)
        {
            var user = new UserInfo() { ChatEndPoint = endPoint };
            return user;
        }

        public void Update(UserInfo user)
        {
            this.UserFriendlyName = user.UserFriendlyName;
            this.DisplayMessage = user.DisplayMessage;
            this.Properties = user.Properties;
            this.Status = user.Status;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is UserInfo)
            {
                IPEndPoint endPoint = ((UserInfo)obj).PresenceEndPoint;
                return endPoint.Equals(PresenceEndPoint);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (this.PresenceEndPoint != null)
                return PresenceEndPoint.GetHashCode();
            return base.GetHashCode();
        }
    }
}
