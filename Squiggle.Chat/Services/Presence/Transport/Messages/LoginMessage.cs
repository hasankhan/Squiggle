using System;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Messages
{
    [Serializable]
    class LoginMessage: Message
    {
        public string UserFriendlyName { get; set; }
        public IPEndPoint ChatEndPoint { get; set; }   
        public TimeSpan KeepAliveSyncTime { get; set; }                
    }
}
