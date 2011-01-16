using System;
using System.Net;
using System.ServiceModel;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat.Services.Presence.Transport.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    class PresenceHost: IPresenceHost
    {
        public UserInfo UserInfo { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceiveMessage(ChatEndPoint sender, byte[] message)
        {
            var msg = Message.Deserialize(message);
            MessageReceived(this, new MessageReceivedEventArgs() { Message = msg, Sender = sender });
        }

        public UserInfo GetUserInfo()
        {
            return UserInfo;
        }
    }
}
