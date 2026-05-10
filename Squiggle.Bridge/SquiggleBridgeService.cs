using System;
using System.Net;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Squiggle.Bridge.Configuration;
using Squiggle.Utilities;

namespace Squiggle.Bridge
{
    partial class SquiggleBridgeService : ConsoleService
    {
        readonly BridgeConfiguration config;
        readonly ILogger<SquiggleBridgeService> _logger;
        SquiggleBridge bridge = null!;

        public SquiggleBridgeService(BridgeConfiguration config, ILogger<SquiggleBridgeService>? logger = null) : base(logger)
        {
            this.config = config;
            this._logger = logger ?? NullLogger<SquiggleBridgeService>.Instance;
            InitializeComponent();
        }                

        protected override void OnStart(string[] args)
        {
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

        void DumpConfig(BridgeConfiguration config, IPEndPoint presenceServiceEndPoint)
        {
            _logger.LogInformation(":: Settings ::");
            _logger.LogInformation("Bridge endpoint (Internal): {Endpoint}", config.InternalServiceBinding.EndPoint);
            _logger.LogInformation("Bridge endpoint (External): {Endpoint}", config.ExternalServiceBinding.EndPoint);
            _logger.LogInformation("Presence multicast endpoint: {Endpoint}", config.PresenceBinding.MulticastEndPoint);
            _logger.LogInformation("Presence endpoint: {Endpoint}", presenceServiceEndPoint);
            _logger.LogInformation(":: Target bridges ::");
            foreach (Target target in config.Targets)
                _logger.LogInformation("Target: {Endpoint}", target.EndPoint);
        }

        protected override void OnStop()
        {
            bridge.Stop();
            bridge = null!;
        }
    }
}
