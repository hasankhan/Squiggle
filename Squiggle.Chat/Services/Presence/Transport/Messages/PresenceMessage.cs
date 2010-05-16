using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Messages
{
    [Serializable]
    abstract class PresenceMessage<TMessage>:Message where TMessage:PresenceMessage<TMessage>, new()
    {
        public string UserFriendlyName { get; set; }
        public string DisplayMessage { get; set; }
        public UserStatus Status { get; set; }
        public IPEndPoint ChatEndPoint { get; set; }
        public TimeSpan KeepAliveSyncTime { get; set; }

        public static TMessage FromUserInfo(UserInfo user) 
        {
            var message = new TMessage()
            {
                ChatEndPoint = user.ChatEndPoint,
                DisplayMessage = user.DisplayMessage,
                Status = user.Status,
                KeepAliveSyncTime = user.KeepAliveSyncTime,
                UserFriendlyName = user.UserFriendlyName
            };
            return message;
        }

        public TSecondMessage Convert<TSecondMessage>() where TSecondMessage : PresenceMessage<TSecondMessage>, new()
        {
            var message = new TSecondMessage();
            message.ChannelID = this.ChannelID;
            message.ChatEndPoint = this.ChatEndPoint;
            message.DisplayMessage = this.DisplayMessage;
            message.KeepAliveSyncTime = this.KeepAliveSyncTime;
            message.Status = this.Status;
            message.UserFriendlyName = this.UserFriendlyName;
            return message;
        }

        public UserInfo GetUser()
        {
            var user = new UserInfo()
            {
                ChatEndPoint = this.ChatEndPoint,
                DisplayMessage = this.DisplayMessage,
                Status = this.Status,
                KeepAliveSyncTime = this.KeepAliveSyncTime,
                UserFriendlyName = this.UserFriendlyName
            };
            return user;
        }
    }
}
