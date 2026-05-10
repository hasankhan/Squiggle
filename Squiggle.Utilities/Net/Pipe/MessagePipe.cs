using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using NetMQ;

namespace Squiggle.Utilities.Net.Pipe
{
    public abstract class MessagePipe : IDisposable
    {
        NetMQSocket listener;
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
        }

        public void Open()
        {
            this.listener = CreateListener();

            listenTask = Task.Factory.StartNew(Listen, listenCancelToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        protected abstract NetMQSocket CreateListener();
        
        void Listen()
        {
            try
            {
                while (!listenCancelToken.Token.IsCancellationRequested)
                {
                    var msg = new Msg();
                    msg.InitEmpty();
                    bool received = this.listener.TryReceive(ref msg, TimeSpan.FromMilliseconds(listenTimeout));
                    if (received && msg.Size > 0)
                    {
                        byte[] data = new byte[msg.Size];
                        Buffer.BlockCopy(msg.Data, msg.Offset, data, 0, msg.Size);
                        MessageReceived(this, new MessageReceivedEventArgs() { Message = data });
                    }
                    msg.Close();
                }
            }
            catch (Exception)
            {
                // finish the task gracefully
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
