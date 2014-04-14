using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NetMQ;

namespace Squiggle.Utilities.Net.Pipe
{
    public class MulticastMessagePipe: MessagePipe
    {
        IPAddress multicastAddress;
        NetMQSocket publisher;

        public MulticastMessagePipe(IPEndPoint bindTo, IPAddress multicastAddress): base(bindTo)
        {
            this.multicastAddress = multicastAddress;
        }

        protected override NetMQSocket CreateListener()
        {
            NetMQSocket listener = this.Context.CreateSubscriberSocket();
            string bindTo = CreateAddress();
            listener.Connect(bindTo);

            return listener;
        }

        public void Send(byte[] message)
        {
            if (publisher == null)
            {
                publisher = this.Context.CreatePublisherSocket();
                publisher.Connect(CreateAddress());
            }

            publisher.Send(message);
        }

        string CreateAddress()
        {
            string host = String.Format("{0};{1}", Host, multicastAddress);
            string bindTo = CreateAddress("epgm", host, Port);
            return bindTo;
        }

        protected override void Dispose(bool disposing)
        {
            if (publisher != null)
            {
                publisher.Dispose();
                publisher = null;
            }

            base.Dispose(disposing);
        }
    }
}
