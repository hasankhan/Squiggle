using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;
using System.Net;

namespace Squiggle.Bridge
{
    class SquiggleBridge
    {
        BridgeHost service = new BridgeHost();
        ServiceHost host;
        PresenceChannel channel;
        List<BridgeHostProxy> targets = new List<BridgeHostProxy>();               

        public SquiggleBridge()
        {
            service.MessageReceived += new EventHandler<MessageReceivedEventArgs>(service_MessageReceived);
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
            host = new ServiceHost(service);
            host.AddServiceEndpoint(typeof(IBridgeHost), binding, address);
            host.Open();
            channel = new PresenceChannel(presenceEndPoint);
            channel.Start();
            channel.MessageReceived += new EventHandler<Chat.Services.Presence.Transport.MessageReceivedEventArgs>(channel_MessageReceived);
        }

        public void Stop()
        {
            channel.Stop();
            host.Close();
            foreach (BridgeHostProxy target in targets)
                target.Dispose();
        }

        void service_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.ChannelID != channel.ChannelID)
            {
                channel.SendMessage(e.Message);
                Console.WriteLine(e.Message.ToString());
            }
        }

        void channel_MessageReceived(object sender, Chat.Services.Presence.Transport.MessageReceivedEventArgs e)
        {
            byte[] message = e.Message.Serialize();
            foreach (BridgeHostProxy target in targets)
                target.ReceiveMessage(message);
        }
    }
}
