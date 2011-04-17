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

            public override bool Equals(object obj)
            {
                if (obj is Client)
                {
                    var other = (Client)obj;
                    return Callback.Equals(other.Callback);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Callback == null ? 0 : Callback.GetHashCode();
            }
        }

        HashSet<Client> clients;

        public MulticastHost()
        {
            this.clients = new HashSet<Client>();
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
                    clients.Remove(new Client(client));
            }, "unregistering client");
        }

        public void ForwardMessage(Message message)
        {
            ExceptionMonster.EatTheException(() =>
            {
                RegisterClient();

                IMulticastServiceCallback currentClient = GetCurrentClient();
                IEnumerable<Client> clientsClone;

                lock (clients)
                    clientsClone = clients.ToList();

                foreach (var client in clientsClone)
                    if (client.Callback != currentClient)
                    {
                        Client current = client; // to make it part of closure
                        Async.Invoke(() =>
                        {
                            try
                            {
                                current.Callback.MessageForwarded(message);
                                current.ErrorCount = 0;
                            }
                            catch (Exception)
                            {
                                current.ErrorCount++;
                                if (current.ErrorCount >= 3)
                                    lock (clients)
                                        clients.Remove(client);
                            }
                        });
                    }

            }, "forwarding message");
        }

        IMulticastServiceCallback GetCurrentClient()
        {
            var client = OperationContext.Current.GetCallbackChannel<IMulticastServiceCallback>();
            return client;
        }
    }
}
