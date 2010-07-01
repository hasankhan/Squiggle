using System;
using System.ServiceProcess;
using Squiggle.Bridge.Configuration;

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
