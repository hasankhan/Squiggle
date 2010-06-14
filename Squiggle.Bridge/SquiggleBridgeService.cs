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
using System.Linq;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;

namespace Squiggle.Bridge
{
    public partial class SquiggleBridgeService : ServiceBase
    {
        SquiggleBridge bridge;

        public SquiggleBridgeService()
        {
            InitializeComponent();
        }                

        protected override void OnStart(string[] args)
        {
            var config = BridgeConfiguration.GetConfig();
            bridge = new SquiggleBridge();
            foreach (Target target in config.Targets)
                bridge.AddTarget(target.EndPoint);
            bridge.Start(config.ServiceBinding.EndPoint, config.ChannelBinding.EndPoint);
        }

        protected override void OnStop()
        {
            bridge.Stop();
            bridge = null;
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
