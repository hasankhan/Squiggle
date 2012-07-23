using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using Squiggle.Core;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService;
using Squiggle.Utilities;
using Squiggle.Core.Presence.Transport.Broadcast.MultcastService.Messages;

namespace Squiggle.Multicast
{
    class MulticastService
    {
        MulticastHost mcastHost;
        HashSet<IPEndPoint> clients;
        IPEndPoint endPoint;

        public MulticastService(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            this.clients = new HashSet<IPEndPoint>();
        }

        public void Start()
        {
            mcastHost = new MulticastHost(endPoint);
            mcastHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(mcastHost_MessageReceived);
            mcastHost.Start();
        }

        void mcastHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            AddClient(e.Message.Sender);
            if (e.Message is MulticastMessage)
                OnMulticastMessage((MulticastMessage)e.Message);
            else if (e.Message is RegisterMessage)
                OnRegisterMessage(e.Message);
            else if (e.Message is UnregisterMessage)
                OnUnregisterMessage((UnregisterMessage)e.Message);
        }

        void OnRegisterMessage(Message message)
        {
            AddClient(message.Sender);
        }

        void OnUnregisterMessage(UnregisterMessage message)
        {
            RemoveClient(message.Sender);
        }

        void OnMulticastMessage(MulticastMessage message)
        {
            AddClient(message.Sender);
            ForwardMessage(message);
        }

        public void Stop()
        {
            lock (clients)
                clients.Clear();

            mcastHost.Dispose();
        }

        void ForwardMessage(MulticastMessage message)
        {
            AddClient(message.Sender);

            IEnumerable<IPEndPoint> clientsList;

            lock (clients)
                clientsList = clients.ToList();

            clients.ForEach(client =>
            {
                if (!client.Equals(message.Sender))
                    mcastHost.Send(client, message);
            });
        }

        void AddClient(IPEndPoint client)
        {
            lock (clients)
                clients.Add(client);
        }

        void RemoveClient(IPEndPoint client)
        {
            lock (clients)
                clients.Remove(client);
        }
    }
}
