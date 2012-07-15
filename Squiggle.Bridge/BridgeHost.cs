using System;
using System.ServiceModel;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Chat;
using System.Net;
using System.Linq;
using Squiggle.Core;
using Squiggle.Core.Presence;
using System.ServiceModel.Dispatcher;
using System.Collections.Generic;
using Squiggle.Core.Chat.Transport.Host;

namespace Squiggle.Bridge
{
    public class PresenceMessageForwardedEventArgs: EventArgs
    {
        public IPEndPoint BridgeEndPoint { get; set; }
        public Message Message {get; set; }
        public SquiggleEndPoint Recipient {get; set; }

        public bool IsBroadcast
        {
            get { return Recipient == null; }
        }

        public PresenceMessageForwardedEventArgs (Message message, IPEndPoint bridgeEdnpoint, SquiggleEndPoint recipient)
	    {
            this.Message = message;
            this.BridgeEndPoint = bridgeEdnpoint;
	        this.Recipient = recipient;
        }
    }

    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public SquiggleEndPoint Recipient { get; set; }
        public Squiggle.Core.Chat.Transport.Message Message { get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] 
    public class BridgeHost: IBridgeHost
    {
        SquiggleBridge bridge;

        public event EventHandler<PresenceMessageForwardedEventArgs> PresenceMessageForwarded = delegate { };
        public event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived = delegate { };

        internal BridgeHost(SquiggleBridge bridge)
        {
            this.bridge = bridge;
        }

        public void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint)
        {
            var msg = Message.Deserialize(message);
            var args = new PresenceMessageForwardedEventArgs(msg, bridgeEndPoint, recipient);
            PresenceMessageForwarded(this, args);
        }

        public void ReceiveChatMessage(SquiggleEndPoint recipient, byte[] message)
        {
            var msg = Squiggle.Core.Chat.Transport.Message.Deserialize(message);
            var args = new ChatMessageReceivedEventArgs() { Message = msg, Recipient = recipient };
            ChatMessageReceived(this, args);
        }
    }
}
