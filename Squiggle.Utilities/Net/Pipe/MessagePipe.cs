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
    public abstract class MessagePipe : IDisposable
    {
        Context context;

        Socket listener;
        Task listenTask;
        CancellationTokenSource listenCancelToken;
        int listenTimeout = (int)1.Seconds().TotalMilliseconds;

        protected string Host { get; private set; }
        protected int Port { get; private set; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public MessagePipe(int port) : this("*", port) { }
        public MessagePipe(IPEndPoint localEndpoint) : this(localEndpoint.Address.ToString(), localEndpoint.Port) { }
        public MessagePipe(string host, int port)
        {
            this.Host = host;
            this.Port = port;

            listenCancelToken = new CancellationTokenSource();
            listenTask = new Task(Listen, listenCancelToken.Token, TaskCreationOptions.LongRunning);
        }

        public void Open()
        {
            context = new Context(2);
            this.listener = CreateListener();

            listenTask.Start(TaskScheduler.Default);
        }

        protected Socket GetSocket(SocketType type)
        {
            var socket = context.Socket(type);
            socket.Linger = 0;
            return socket;
        }

        protected abstract Socket CreateListener();
        
        void Listen()
        {
            while (!listenCancelToken.Token.IsCancellationRequested)
            {
                byte[] data = listener.Recv(listenTimeout);
                if (data != null)
                    MessageReceived(this, new MessageReceivedEventArgs() { Message = data });
            }
        }

        protected static string CreateAddress(string protocol, string host, int port)
        {
            string bindTo = String.Format("{0}://{1}:{2}", protocol, host, port);
            return bindTo;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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

            listener.Dispose();
            context.Dispose();
        }

        ~MessagePipe()
        {
            Dispose(false);
        }
    }
}
