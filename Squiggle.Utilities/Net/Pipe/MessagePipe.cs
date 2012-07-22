using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZMQ;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace Squiggle.Utilities.Net.Pipe
{
    public class MessagePipe : IDisposable
    {
        Socket listener;
        Context context;
        Dictionary<string, Socket> sockets;
        Task listenTask;
        CancellationTokenSource listenCancelToken;
        int listenTimeout = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public MessagePipe(int port) : this("*", port) { }
        public MessagePipe(IPEndPoint localEndpoint) : this(localEndpoint.Address.ToString(), localEndpoint.Port) { }
        public MessagePipe(string host, int port)
        {
            this.sockets = new Dictionary<string, Socket>();

            listenCancelToken = new CancellationTokenSource();
            listenTask = new Task(Listen, listenCancelToken.Token, TaskCreationOptions.LongRunning);

            context = new Context(6);
            string bindTo = CreateAddress(host, port);
            this.listener = context.Socket(SocketType.PULL);
            this.listener.Bind(bindTo);
        }

        public void Open()
        {
            listenTask.Start(TaskScheduler.Default);
        }

        public void Send(IPEndPoint target, byte[] message)
        {
            Send(target.Address.ToString(), target.Port, message);
        }

        public void Send(string host, int port, byte[] message)
        {
            string target = CreateAddress(host, port);
            Socket socket;
            lock (sockets)
                if (!sockets.TryGetValue(target, out socket))
                {
                    sockets[target] = socket = context.Socket(SocketType.PUSH);
                    socket.Connect(target);
                }

            socket.Send(message);
        }

        void Listen()
        {
            while (!listenCancelToken.Token.IsCancellationRequested)
            {
                byte[] data = listener.Recv(listenTimeout);
                if (data != null)
                    MessageReceived(this, new MessageReceivedEventArgs() { Message = data });
            }
        }

        static string CreateAddress(string host, int port)
        {
            string bindTo = String.Format("tcp://{0}:{1}", host, port);
            return bindTo;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!listenTask.IsCompleted)
            {
                listenCancelToken.Cancel();
                try
                {
                    listenTask.Wait();
                }
                catch { }
            }

            foreach (Socket socket in sockets.Values)
                socket.Dispose();

            listener.Dispose();
            context.Dispose();
        }

        ~MessagePipe()
        {
            Dispose(false);
        }
    }
}
