using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Core.Presence.Transport;
using System.ServiceModel;
using Squiggle.Utilities;
using System.Diagnostics;
using System.Threading.Tasks;

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
            RegisterClient();

            Client currentClient = GetCurrentClient();
            IEnumerable<Client> clientsList;

            lock (clients)
                clientsList = clients.ToList();
            
            Parallel.ForEach(clientsList, client =>
            {
                if (!client.Equals(currentClient))
                {
                    CommunicationState state = client.Channel.State;
                    if (state == CommunicationState.Opened)
                        client.Callback.MessageForwarded(message);
                    else
                    {
                        Trace.WriteLine("Removing faulty client");
                        RemoveClient(client);
                    }
                }
            });
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

        Client GetCurrentClient()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IMulticastServiceCallback>();
            var client = new Client(callback, OperationContext.Current.Channel);
            return client;
        }

        class Client
        {
            public IMulticastServiceCallback Callback { get; set; }
            public IContextChannel Channel { get; set; }

            public Client(IMulticastServiceCallback callback, IContextChannel channel)
            {
                Callback = callback;
                Channel = channel;
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
