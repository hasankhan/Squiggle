using System;
using System.Collections.Generic;
using System.Net;
using ProtoBuf;

namespace Squiggle.Core.Presence.Transport.Messages
{
    [ProtoContract]
    [ProtoInclude(50, typeof(HiMessage))]
    [ProtoInclude(51, typeof(HelloMessage))]
    [ProtoInclude(52, typeof(UserInfoMessage))]
    public abstract class PresenceMessage : Message
    {        
        [ProtoMember(1)]
        public string DisplayName { get; set; }
        [ProtoMember(2)]        
        public UserStatus Status { get; set; }
        [ProtoMember(3)]
        public Dictionary<string, string> Properties { get; set; }
        [ProtoMember(4)]
        public TimeSpan KeepAliveSyncTime { get; set; }
        [ProtoMember(5)]
        IPAddress ChatIP { get; set; }
        [ProtoMember(6)]
        int ChatPort { get; set; }

        public IPEndPoint ChatEndPoint
        {
            get { return new IPEndPoint(ChatIP, ChatPort); }
            set
            {
                ChatIP = value.Address;
                ChatPort = value.Port;
            }
        }

        public static new TMessage FromUserInfo<TMessage>(UserInfo user) where TMessage: PresenceMessage, new ()
        {
            var message = new TMessage()            
            {   
                ClientID = user.ID,
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
                ID = this.ClientID,
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
