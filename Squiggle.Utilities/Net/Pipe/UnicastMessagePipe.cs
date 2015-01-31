using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using NetMQ;

namespace Squiggle.Utilities.Net.Pipe
{
    public class UnicastMessagePipe: MessagePipe
    {
        Dictionary<string, NetMQSocket> sockets;

        public UnicastMessagePipe(IPEndPoint localEndPoint) : base(localEndPoint)
        {
            this.sockets = new Dictionary<string, NetMQSocket>();
        }

        protected override NetMQSocket CreateListener()
        {
            NetMQSocket listener = this.Context.CreatePullSocket();
            string bindTo = CreateAddress("tcp", Host, Port);
            listener.Bind(bindTo);

            return listener;
        }

        public void Send(IPEndPoint target, byte[] message)
        {
            Validator.IsNotNull(target, "target");
            Validator.IsNotNull(message, "message");
            Send(target.Address.ToString(), target.Port, message);
        }

        public void Send(string host, int port, byte[] message)
        {
            string target = CreateAddress("tcp", host, port);
            NetMQSocket socket;
            lock (sockets)
                if (!sockets.TryGetValue(target, out socket))
                {
                    sockets[target] = socket = this.Context.CreatePushSocket();
                    socket.Connect(target);
                }

            socket.Send(message);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (NetMQSocket socket in sockets.Values)
                socket.Dispose();

            base.Dispose(disposing);
        }
    }
}
