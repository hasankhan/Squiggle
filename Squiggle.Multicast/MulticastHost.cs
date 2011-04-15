using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Chat.Services.Presence.Transport;
using System.ServiceModel;
using Squiggle.Utilities;

namespace Squiggle.Multicast
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] 
    public class MulticastHost : IMulticastService
    {
        class Client
        {
            public IMulticastServiceCallback Callback { get; set; }
            public int ErrorCount { get; set; }

            public Client(IMulticastServiceCallback callback)
            {
                Callback = callback;
            }
        }

        List<Client> clients;

        public MulticastHost()
        {
            this.clients = new List<Client>();
        }

        public void Reset()
        {
            lock (clients)
                clients.Clear();
        }        

        public void RegisterClient()
        {
            ExceptionMonster.EatTheException(() =>
            {
                lock (clients)
                    clients.Add(new Client(GetCurrentClient()));
            }, "registering client");
        }

        public void UnRegisterClient()
        {
            ExceptionMonster.EatTheException(() =>
            {
                var client = GetCurrentClient();
                lock (clients)
                    clients.Remove(clients.FirstOrDefault(c => c.Callback == client));
            }, "unregistering client");
        }


        public void ForwardMessage(Message message)
        {
            ExceptionMonster.EatTheException(() =>
            {
                IMulticastServiceCallback currentClient = GetCurrentClient();
                IEnumerable<Client> clientsClone;

                lock (clients)
                    clientsClone = clients.ToList();

                Async.Invoke(() =>
                {
                    foreach (var client in clientsClone)
                        if (client.Callback != currentClient)
                            try
                            {
                                client.Callback.MessageForwarded(message);
                                client.ErrorCount = 0;
                            }
                            catch (Exception)
                            {
                                client.ErrorCount++;
                                if (client.ErrorCount >= 3)
                                    clients.Remove(client);
                            }
                });

            }, "forwarding message");
        }

        IMulticastServiceCallback GetCurrentClient()
        {
            var client = OperationContext.Current.GetCallbackChannel<IMulticastServiceCallback>();
            return client;
        }
    }
}
