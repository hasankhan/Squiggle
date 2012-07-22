using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using Squiggle.Core;
using Squiggle.Core.Chat;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Core.Presence;
using Squiggle.Utilities.Net.Pipe;
using Squiggle.Utilities.Serialization;
using Squiggle.Bridge.Messages;

namespace Squiggle.Bridge
{
    public class PresenceMessageForwardedEventArgs: EventArgs
    {
        public IPEndPoint BridgeEndPoint { get; set; }
        public Squiggle.Core.Presence.Transport.Message Message {get; set; }

        public bool IsBroadcast
        {
            get { return Message.Recipient == null; }
        }

        public PresenceMessageForwardedEventArgs(Squiggle.Core.Presence.Transport.Message message, IPEndPoint bridgeEdnpoint)
	    {
            this.Message = message;
            this.BridgeEndPoint = bridgeEdnpoint;
        }
    }

    public class ChatMessageReceivedEventArgs : EventArgs
    {
        public Squiggle.Core.Chat.Transport.Message Message { get; set; }
    }

    public class BridgeHost: IDisposable
    {
        IPEndPoint externalEndPoint;
        IPEndPoint internalEndPoint;
        MessagePipe bridgePipe;
        SquiggleBridge bridge;
        MessagePipe chatPipe;

        public event EventHandler<PresenceMessageForwardedEventArgs> PresenceMessageForwarded = delegate { };
        public event EventHandler<ChatMessageReceivedEventArgs> ChatMessageReceived = delegate { };

        internal BridgeHost(SquiggleBridge bridge, IPEndPoint externalEndPoint, IPEndPoint internalEndPoint)
        {
            this.bridge = bridge;
            this.externalEndPoint = externalEndPoint;
            this.internalEndPoint = internalEndPoint;
        }

        public void Start()
        {
            bridgePipe = new MessagePipe(externalEndPoint);
            bridgePipe.MessageReceived += new EventHandler<Utilities.Net.Pipe.MessageReceivedEventArgs>(bridgePipe_MessageReceived);
            bridgePipe.Open();

            chatPipe = new MessagePipe(internalEndPoint);
            chatPipe.MessageReceived += new EventHandler<Utilities.Net.Pipe.MessageReceivedEventArgs>(chatPipe_MessageReceived);
            chatPipe.Open();
        }

        void chatPipe_MessageReceived(object sender,Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            OnChatMessage(e.Message);
        }

        void bridgePipe_MessageReceived(object sender, Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            var message = Message.Deserialize(e.Message);
            if (message is ForwardPresenceMessage)
                OnPresenceMessage((ForwardPresenceMessage)message);
            else if (message is ForwardChatMessage)
                OnChatMessage(((ForwardChatMessage)message).Message);
        }

        void OnPresenceMessage(ForwardPresenceMessage message)
        {
            var msg = Squiggle.Core.Presence.Transport.Message.Deserialize(message.Message);
            var args = new PresenceMessageForwardedEventArgs(msg, message.BridgeEndPoint);
            PresenceMessageForwarded(this, args);
        }

        void OnChatMessage(byte[] message)
        {
            var msg = Squiggle.Core.Chat.Transport.Message.Deserialize(message);
            var args = new ChatMessageReceivedEventArgs() { Message = msg};
            ChatMessageReceived(this, args);
        }

        public void Dispose()
        {
            if (bridgePipe != null)
            {
                bridgePipe.Dispose();
                bridgePipe = null;
            }

            if (chatPipe != null)
            {
                chatPipe.Dispose();
                chatPipe = null;
            }
        }

        public void SendChatMessage(bool local, IPEndPoint target, Core.Chat.Transport.Message message)
        {
            if (local)
                chatPipe.Send(target, message.Serialize());
            else
                Send(target, new ForwardChatMessage() { Message = message.Serialize() });
        }

        public void SendPresenceMessage(IPEndPoint target, byte[] message)
        {
            Send(target, new ForwardPresenceMessage() { BridgeEndPoint = externalEndPoint, Message = message});
        }

        void Send(IPEndPoint target, Message message)
        {
            bridgePipe.Send(target, message.Serialize());
        }
    }
}
