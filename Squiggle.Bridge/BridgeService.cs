using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Bridge
{
    public class MessageReceivedEventArgs: EventArgs
    {
        public Message Message {get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class BridgeService: IBridgeService
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceiveMessage(byte[] message)
        {
            var msg = Message.Deserialize(message);
            MessageReceived(this, new MessageReceivedEventArgs() { Message = msg });
        }
    }
}
