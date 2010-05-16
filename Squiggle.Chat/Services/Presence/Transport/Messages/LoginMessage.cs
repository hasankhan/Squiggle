using System;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Messages
{
    [Serializable]
    class LoginMessage: Message
    {
        public string UserFriendlyName { get; set; }
        public string DisplayMessage { get; set; }
        public UserStatus Status { get; set; }
        public IPEndPoint ChatEndPoint { get; set; }   
        public TimeSpan KeepAliveSyncTime { get; set; }

        public static LoginMessage FromUserInfo(UserInfo user)
        {
            var message = new LoginMessage()
            {
                ChatEndPoint = user.ChatEndPoint,
                DisplayMessage = user.DisplayMessage,
                Status = user.Status,
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
                DisplayMessage = this.DisplayMessage,
                Status = this.Status,
                KeepAliveSyncTime = this.KeepAliveSyncTime,
                UserFriendlyName = this.UserFriendlyName
            };
            return user;
        }
    }
}
