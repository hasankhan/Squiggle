using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;

namespace Squiggle.Chat.Services.Presence.Transport.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    class PresenceHost: IPresenceHost
    {
        public UserInfo UserInfo { get; set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceiveMessage(IPEndPoint sender, byte[] message)
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
