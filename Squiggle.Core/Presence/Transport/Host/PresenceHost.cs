using System;
using System.Net;
using System.ServiceModel;
using Squiggle.Core.Chat;

namespace Squiggle.Core.Presence.Transport.Host
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] 
    class PresenceHost: IPresenceHost
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceivePresenceMessage(SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] message)
        {
            var msg = Message.Deserialize(message);
            MessageReceived(this, new MessageReceivedEventArgs() 
            { 
                Recipient = recepient,
                Message = msg, 
                Sender = sender 
            });
        }
    }
}
