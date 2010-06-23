using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace Squiggle.Chat.Services.Presence.Transport.Messages
{
    [Serializable]
    public abstract class PresenceMessage: Message
    {
        public IPEndPoint ChatEndPoint { get; set; }
        public string UserFriendlyName { get; set; }
        public string DisplayMessage { get; set; }
        public UserStatus Status { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public TimeSpan KeepAliveSyncTime { get; set; }

        public static new TMessage FromUserInfo<TMessage>(UserInfo user) where TMessage: PresenceMessage, new ()
        {
            var message = new TMessage()            
            {   
                ChatEndPoint = user.ChatEndPoint,
                PresenceEndPoint = user.PresenceEndPoint,
                DisplayMessage = user.DisplayMessage,
                Status = user.Status,                
                Properties = user.Properties,
                KeepAliveSyncTime = user.KeepAliveSyncTime,
                UserFriendlyName = user.UserFriendlyName
            };
            return message;
        }

        public UserInfo GetUser()
        {
            var user = new UserInfo()
            {
                ChatEndPoint = this.ChatEndPoint,
                PresenceEndPoint = this.PresenceEndPoint,
                DisplayMessage = this.DisplayMessage,
                Status = this.Status,
                Properties = this.Properties, 
                KeepAliveSyncTime = this.KeepAliveSyncTime,
                UserFriendlyName = this.UserFriendlyName
            };
            return user;
        }
    }
}
