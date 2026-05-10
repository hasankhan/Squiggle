using System;
using System.Diagnostics;
using System.Net;
using Squiggle.Utilities;

namespace Squiggle.Multicast
{
    partial class SquiggleMulticastService : ConsoleService
    {
        readonly IPEndPoint endPoint;
        MulticastServer service = null!;

        public SquiggleMulticastService(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            DumpConfig(endPoint);

            service = new MulticastServer(endPoint);
            service.Start();
        }

        void DumpConfig(IPEndPoint endPoint)
        {
            Trace.WriteLine(":: Settings ::");
            Trace.WriteLine("");
            Trace.WriteLine("Endpoint: " + endPoint);
            Trace.WriteLine("");            
        }

        protected override void OnStop()
        {
            service.Stop();
            service = null!;
        }
    }
}
