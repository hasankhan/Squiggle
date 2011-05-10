using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat.Services.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Chat.Services.Presence.Transport;
using System.ServiceModel;
using Squiggle.Utilities;
using System.Diagnostics;

namespace Squiggle.Multicast
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] 
    public class MulticastHost : IMulticastService
    {
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
                var client = GetCurrentClient();
                AddClient(client);
            }, "registering client");
        }        

        public void UnRegisterClient()
        {
            ExceptionMonster.EatTheException(() =>
            {
                var client = GetCurrentClient();
                RemoveClient(client);
            }, "unregistering client");
        }

        public void ForwardMessage(Message message)
        {
            ExceptionMonster.EatTheException(() =>
            {
                RegisterClient();

                Client currentClient = GetCurrentClient();
                IEnumerable<Client> clientsClone;

                lock (clients)
                    clientsClone = clients.ToList();

                foreach (var client in clientsClone)
                    if (!client.Equals(currentClient))
                    {
                        Client current = client; // to make it part of closure
                        Async.Invoke(() =>
                        {
                            if (!ForwardMessage(message, current))
                            {
                                current.ErrorCount++;
                                if (current.ErrorCount >= 3)
                                {
                                    Trace.WriteLine("Removing faulty client");
                                    RemoveClient(client);
                                }
                            }
                        });
                    }

            }, "forwarding message");
        }

        void AddClient(Client client)
        {
            lock (clients)
                clients.Add(client);
        }
        
        void RemoveClient(Client client)
        {
            lock (clients)
                clients.Remove(client);
        }

        static bool ForwardMessage(Message message, Client current)
        {
            return ExceptionMonster.EatTheException(() =>
            {
                current.Callback.MessageForwarded(message);
                current.ErrorCount = 0;
            }, "forwarding message");
        }

        Client GetCurrentClient()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IMulticastServiceCallback>();
            var client = new Client(callback);
            return client;
        }

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
    }
}
