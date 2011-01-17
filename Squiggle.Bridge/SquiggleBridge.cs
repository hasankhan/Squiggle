using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Bridge
{
    class SquiggleBridge
    {
        BridgeHost bridgeHost = new BridgeHost();
        ServiceHost host;
        PresenceChannel presenceChannel;
        List<BridgeHostProxy> targets = new List<BridgeHostProxy>();               

        public SquiggleBridge()
        {
            bridgeHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(bridgeHost_MessageReceived);
        }

        public void AddTarget(IPEndPoint target)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var address = new Uri("net.tcp://" + target.ToString() + "/squigglebridge");
            var proxy = new BridgeHostProxy(binding, new EndpointAddress(address));
            targets.Add(proxy);
        }

        public void Start(IPEndPoint bridgeEndPoint, IPEndPoint presenceEndPoint)
        {
            var address = new Uri("net.tcp://" + bridgeEndPoint + "/squigglebridge");
            var binding = new NetTcpBinding(SecurityMode.None);
            host = new ServiceHost(bridgeHost);
            host.AddServiceEndpoint(typeof(IBridgeHost), binding, address);
            host.Open();
            presenceChannel = new PresenceChannel(presenceEndPoint, new IPEndPoint(bridgeEndPoint.Address, presenceEndPoint.Port));
            presenceChannel.Start();
            presenceChannel.MessageReceived += new EventHandler<Chat.Services.Presence.Transport.MessageReceivedEventArgs>(presenceChannel_MessageReceived);
        }

        public void Stop()
        {
            presenceChannel.Stop();
            host.Close();
            foreach (BridgeHostProxy target in targets)
                target.Dispose();
        }

        void bridgeHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.ChannelID != presenceChannel.ChannelID)
            {                
                presenceChannel.SendMessage(e.Message);
                Console.WriteLine(e.Message.ToString());
            }
        }

        void presenceChannel_MessageReceived(object sender, Chat.Services.Presence.Transport.MessageReceivedEventArgs e)
        {
            byte[] message = e.Message.Serialize();
            foreach (BridgeHostProxy target in targets)
                target.ForwardPresenceMessage(message);
        }
    }
}
