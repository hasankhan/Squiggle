using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities.Net.Pipe;
using System.Net;

namespace Squiggle.Core.Presence.Transport
{
    class PresenceHost: IDisposable
    {
        MessagePipe pipe;
        IPEndPoint endpoint;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public PresenceHost(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public void Start()
        {
            pipe = new MessagePipe(endpoint);
            pipe.MessageReceived += new EventHandler<Utilities.Net.Pipe.MessageReceivedEventArgs>(pipe_MessageReceived);
            pipe.Open();
        }

        public void Send(Message message)
        {
            byte[] data = message.Serialize();
            pipe.Send(message.Recipient.Address, data);
        }

        void pipe_MessageReceived(object sender, Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            var msg = Message.Deserialize(e.Message);
            MessageReceived(this, new MessageReceivedEventArgs() { Message = msg });
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
