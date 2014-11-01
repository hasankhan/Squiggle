using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using NetMQ;
using NetMQ.zmq;

namespace Squiggle.Utilities.Net.Pipe
{
    public abstract class MessagePipe : IDisposable
    {
        NetMQSocket listener;
        Task listenTask;
        CancellationTokenSource listenCancelToken;
        int listenTimeout = (int)1.Seconds().TotalMilliseconds;

        protected NetMQContext Context { get; private set; }

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
            Context = NetMQContext.Create();
            this.listener = CreateListener();

            listenTask.Start(TaskScheduler.Default);
        }

        protected abstract NetMQSocket CreateListener();
        
        void Listen()
        {
            try
            {
                while (!listenCancelToken.Token.IsCancellationRequested)
                {
                    byte[] data = this.listener.Receive();
                    if (data != null)
                        MessageReceived(this, new MessageReceivedEventArgs() { Message = data });
                }
            }
            catch (TerminatingException)
            {
                // fine just finish the task gracefully
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
                if (listener != null)
                    listener.Dispose();
                this.Context.Dispose();
                try
                {
                    listenTask.Wait(listenTimeout);
                }
                catch { }
            }            
        }

        ~MessagePipe()
        {
            Dispose(false);
        }
    }
}
