using System;
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using Squiggle.Bridge.Configuration;
using Squiggle.Utilities;

namespace Squiggle.Bridge
{
    public partial class SquiggleBridgeService : ConsoleService
    {
        SquiggleBridge bridge;

        public SquiggleBridgeService()
        {
            InitializeComponent();
        }                

        protected override void OnStart(string[] args)
        {
            var config = BridgeConfiguration.GetConfig();

            var presenceServiceEndPoint = new IPEndPoint(config.InternalServiceBinding.EndPoint.Address, config.PresenceBinding.ServicePort);
            var multicastReceiveEndPoint = new IPEndPoint(config.InternalServiceBinding.EndPoint.Address, config.PresenceBinding.CallbackPort);

            DumpConfig(config, presenceServiceEndPoint);

            bridge = new SquiggleBridge(config.InternalServiceBinding.EndPoint,
                                        config.ExternalServiceBinding.EndPoint,
                                        config.PresenceBinding.MulticastEndPoint,
                                        multicastReceiveEndPoint,
                                        presenceServiceEndPoint);

            foreach (Target target in config.Targets)
                bridge.AddTarget(target.EndPoint);

            bridge.Start();
        }

        static void DumpConfig(BridgeConfiguration config, IPEndPoint presenceServiceEndPoint)
        {
            Trace.WriteLine(":: Settings ::");
            Trace.WriteLine("");
            Trace.WriteLine("Bridge endpoint (Internal): " + config.InternalServiceBinding.EndPoint);
            Trace.WriteLine("Bridge endpoint (External): " + config.ExternalServiceBinding.EndPoint);
            Trace.WriteLine("Presence multicast endpoint: " + config.PresenceBinding.MulticastEndPoint);
            Trace.WriteLine("Presence endpoint: " + presenceServiceEndPoint);
            Trace.WriteLine("");
            Trace.WriteLine(":: Target bridges ::");
            Trace.WriteLine("");
            foreach (Target target in config.Targets)
                Trace.WriteLine(target.EndPoint);
            Trace.WriteLine("");
        }

        protected override void OnStop()
        {
            bridge.Stop();
            bridge = null;
        }
    }
}
