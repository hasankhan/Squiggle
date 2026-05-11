using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Presence.Transport.Messages
{
    public abstract class PresenceMessage : Message
    {        
        public string DisplayName { get; set; } = null!;
        public UserStatus Status { get; set; }
        public IDictionary<string, string> Properties { get; set; }
        public TimeSpan KeepAliveSyncTime { get; set; }

        [JsonConverter(typeof(IPEndPointJsonConverter))]
        public IPEndPoint ChatEndPoint { get; set; } = null!;

        public PresenceMessage()
        {
            Properties = new Dictionary<string, string>();
        }

        public static TMessage FromUserInfo<TMessage>(IUserInfo user) where TMessage: PresenceMessage, new ()
        {
            var message = new TMessage()            
            {   
                ChatEndPoint = user.ChatEndPoint,
                Sender = new SquiggleEndPoint(user.ID, user.PresenceEndPoint),
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
                ID = this.Sender.ClientID,
                ChatEndPoint = this.ChatEndPoint,
                PresenceEndPoint = this.Sender.Address,
                Status = this.Status,
                Properties = this.Properties, 
                KeepAliveSyncTime = this.KeepAliveSyncTime,
                DisplayName = this.DisplayName
            };
            return user;
        }
    }
}
