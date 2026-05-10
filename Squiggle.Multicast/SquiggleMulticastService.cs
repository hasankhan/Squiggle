using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Squiggle.Utilities;

namespace Squiggle.Multicast
{
    partial class SquiggleMulticastService : ConsoleService
    {
        readonly IPEndPoint endPoint;
        readonly ILogger<SquiggleMulticastService> _logger;
        MulticastServer service = null!;

        public SquiggleMulticastService(IPEndPoint endPoint, ILogger<SquiggleMulticastService>? logger = null) : base(logger)
        {
            this.endPoint = endPoint;
            this._logger = logger ?? NullLogger<SquiggleMulticastService>.Instance;
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
            _logger.LogInformation(":: Settings ::");
            _logger.LogInformation("Endpoint: {Endpoint}", endPoint);
        }

        protected override void OnStop()
        {
            service.Stop();
            service = null!;
        }
    }
}
