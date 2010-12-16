using System;
using System.Collections.Generic;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Messages
{
    [Serializable]
    public abstract class PresenceMessage: Message
    {
        public IPEndPoint ChatEndPoint { get; set; }
        public string DisplayName { get; set; }
        public UserStatus Status { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public TimeSpan KeepAliveSyncTime { get; set; }

        public static new TMessage FromUserInfo<TMessage>(UserInfo user) where TMessage: PresenceMessage, new ()
        {
            var message = new TMessage()            
            {   
                ChatEndPoint = user.ChatEndPoint,
                PresenceEndPoint = user.PresenceEndPoint,
                Status = user.Status,                
                Properties = user.Properties,
                KeepAliveSyncTime = user.KeepAliveSyncTime,
                DisplayName = user.DisplayName
            };
            return message;
        }

        public UserInfo GetUser()
        {
            var user = new UserInfo()
            {
                ChatEndPoint = this.ChatEndPoint,
                PresenceEndPoint = this.PresenceEndPoint,
                Status = this.Status,
                Properties = this.Properties, 
                KeepAliveSyncTime = this.KeepAliveSyncTime,
                DisplayName = this.DisplayName
            };
            return user;
        }
    }
}
