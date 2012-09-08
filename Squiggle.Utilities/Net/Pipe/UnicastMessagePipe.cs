using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using System.Net;

namespace Squiggle.Utilities.Net.Pipe
{
    public class UnicastMessagePipe: MessagePipe
    {
        Dictionary<string, Socket> sockets;

        public UnicastMessagePipe(IPEndPoint localEndPoint) : base(localEndPoint)
        {
            this.sockets = new Dictionary<string, Socket>();
        }

        protected override Socket CreateListener()
        {
            var listener = GetSocket(SocketType.PULL);
            string bindTo = CreateAddress("tcp", Host, Port);
            listener.Bind(bindTo);

            return listener;
        }

        public void Send(IPEndPoint target, byte[] message)
        {
            Send(target.Address.ToString(), target.Port, message);
        }

        public void Send(string host, int port, byte[] message)
        {
            string target = CreateAddress("tcp", host, port);
            Socket socket;
            lock (sockets)
                if (!sockets.TryGetValue(target, out socket))
                {
                    sockets[target] = socket = GetSocket(SocketType.PUSH);
                    socket.Connect(target);
                }

            socket.Send(message);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (Socket socket in sockets.Values)
                socket.Dispose();

            base.Dispose(disposing);
        }
    }
}
