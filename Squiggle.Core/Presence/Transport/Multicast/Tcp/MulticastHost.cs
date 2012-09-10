using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Presence.Transport.Multicast.Tcp;
using Squiggle.Utilities;
using System.Net;
using Squiggle.Utilities.Net.Pipe;
using Squiggle.Utilities.Serialization;
using Squiggle.Core.Presence.Transport.Multicast.Tcp.Messages;

namespace Squiggle.Core.Presence.Transport.Multicast.Tcp
{
    public class MessageReceivedEventArgs: EventArgs
    {
        public Squiggle.Core.Presence.Transport.Multicast.Tcp.Message Message { get; set; }
    }

    public class MulticastHost: IDisposable
    {
        IPEndPoint endpoint;
        UnicastMessagePipe pipe;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        
        public MulticastHost(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public void Start()
        {
            pipe = new UnicastMessagePipe(endpoint);
            pipe.MessageReceived += new EventHandler<Utilities.Net.Pipe.MessageReceivedEventArgs>(pipe_MessageReceived);
            pipe.Open();
        }

        void pipe_MessageReceived(object sender, Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            SerializationHelper.Deserialize<Squiggle.Core.Presence.Transport.Multicast.Tcp.Message>(e.Message, msg =>
            {
                MessageReceived(this, new MessageReceivedEventArgs() { Message = msg });
            }
            ,"multicast message");
        }

        public void Send(IPEndPoint target, Message message)
        {
            byte[] data = SerializationHelper.Serialize(message);
            pipe.Send(target, data);
        }

        public void Dispose()
        {
            pipe.Dispose();
        }
    }
}
