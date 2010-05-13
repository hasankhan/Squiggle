using System;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport
{
    [Serializable]
    class LoginMessage: Message
    {
        public string UserFriendlyName { get; set; }
        public IPEndPoint ChatEndPoint { get; set; }   
        public int KeepAliveSyncTime { get; set; }                
    }
}
