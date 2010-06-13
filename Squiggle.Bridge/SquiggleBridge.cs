using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Squiggle.Bridge.Configuration;
using System.Net;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Bridge
{
    public partial class SquiggleBridge : ServiceBase
    {
        BridgeService service = new BridgeService();
        ServiceHost host;
        PresenceChannel channel;
        List<BridgeServiceProxy> targets = new List<BridgeServiceProxy>();

        public SquiggleBridge()
        {
            InitializeComponent();
            service.MessageReceived += new EventHandler<MessageReceivedEventArgs>(service_MessageReceived);
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
            foreach (BridgeServiceProxy target in targets)
                target.ReceiveMessage(message);
        }

        protected override void OnStart(string[] args)
        {
            var config = BridgeConfiguration.GetConfig();
            var address = new Uri("net.tcp://" + config.ServiceBinding.EndPoint.ToString() + "/squigglebridge");
            var binding = new NetTcpBinding(SecurityMode.None);
            host = new ServiceHost(service);
            host.AddServiceEndpoint(typeof(IBridgeService), binding, address);
            host.Open();
            foreach (Target target in config.Targets)
            {
                address = new Uri("net.tcp://" + target.EndPoint.ToString() + "/squigglebridge");
                var proxy = new BridgeServiceProxy(binding, new EndpointAddress(address));
                targets.Add(proxy);
            }
            channel = new PresenceChannel(config.ChannelBinding.EndPoint);
            channel.Start();
            channel.MessageReceived += new EventHandler<Chat.Services.Presence.Transport.MessageReceivedEventArgs>(channel_MessageReceived);
        }

        protected override void OnStop()
        {
            channel.Stop();
            host.Close();
            foreach (BridgeServiceProxy target in targets)
                target.Dispose();
            targets.Clear();
        }

        public void RunConsole(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Bridge running... Press any key to stop");
            Console.ReadKey();
            OnStop();
        }
    }
}
