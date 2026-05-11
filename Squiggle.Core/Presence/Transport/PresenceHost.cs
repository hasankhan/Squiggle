using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Squiggle.Utilities.Serialization;
using Squiggle.Core.Presence.Transport.Messages;

namespace Squiggle.Core.Presence.Transport
{
    class PresenceHost : IDisposable
    {
        TcpListener? listener;
        IPEndPoint endpoint;
        CancellationTokenSource? cts;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public PresenceHost(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public void Start()
        {
            cts = new CancellationTokenSource();
            listener = new TcpListener(endpoint);
            listener.Start();
            _ = AcceptClientsAsync(cts.Token);
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await listener!.AcceptTcpClientAsync(token);
                    _ = HandleClientAsync(client);
                }
                catch (OperationCanceledException) { break; }
                catch { /* connection error, continue accepting */ }
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var lengthBuffer = new byte[4];
                    int read = await stream.ReadAsync(lengthBuffer, 0, 4);
                    if (read < 4) return;

                    int length = BitConverter.ToInt32(lengthBuffer, 0);
                    if (length <= 0 || length > 1_000_000) return;

                    var data = new byte[length];
                    int totalRead = 0;
                    while (totalRead < length)
                    {
                        read = await stream.ReadAsync(data, totalRead, length - totalRead);
                        if (read == 0) return;
                        totalRead += read;
                    }

                    SerializationHelper.Deserialize<Message>(data, msg =>
                    {
                        MessageReceived(this, new MessageReceivedEventArgs() { Message = msg });
                    }, "presence message");
                }
            }
            catch { /* message handling error */ }
        }

        public void Send(Message message)
        {
            byte[] data = SerializationHelper.Serialize(message);
            try
            {
                using var client = new TcpClient();
                client.Connect(message.Recipient.Address);
                using var stream = client.GetStream();
                var lengthBytes = BitConverter.GetBytes(data.Length);
                stream.Write(lengthBytes, 0, 4);
                stream.Write(data, 0, data.Length);
            }
            catch { /* send error */ }
        }

        public void Dispose()
        {
            cts?.Cancel();
            listener?.Stop();
            cts?.Dispose();
        }
    }
}
