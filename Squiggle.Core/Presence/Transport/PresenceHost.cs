using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities.Net.Pipe;
using System.Net;
using Squiggle.Utilities.Serialization;
using Squiggle.Core.Presence.Transport.Messages;

namespace Squiggle.Core.Presence.Transport
{
    class PresenceHost: IDisposable
    {
        UnicastMessagePipe pipe;
        IPEndPoint endpoint;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public PresenceHost(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public void Start()
        {
            pipe = new UnicastMessagePipe(endpoint);
            pipe.MessageReceived += pipe_MessageReceived;
            pipe.Open();
        }

        public void Send(Message message)
        {
            byte[] data = SerializationHelper.Serialize(message);
            pipe.Send(message.Recipient.Address, data);
        }

        void pipe_MessageReceived(object sender, Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            SerializationHelper.Deserialize<Message>(e.Message, msg =>
            {
                MessageReceived(this, new MessageReceivedEventArgs() { Message = msg });

            }, "presence message");
        }

        public void Dispose()
        {
            if (pipe != null)
            {
                pipe.Dispose();
                pipe = null;
            }
        }
    }
}
