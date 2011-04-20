using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Configuration;

namespace Squiggle.Utilities
{
    public static class WcfConfig
    {
        public static Binding CreateBinding()
        {
            var binding = new NetTcpBinding(SecurityMode.None);

            binding.MaxReceivedMessageSize = 65535;
            binding.ReaderQuotas.MaxArrayLength = 65535;
            binding.MaxConnections = 100;
            binding.OpenTimeout = TimeSpan.FromSeconds(5);
            binding.CloseTimeout = TimeSpan.FromSeconds(5);

            return binding;
        }

        public static void ConfigureHost(ServiceHost host)
        {            
            host.Description.Behaviors.Add(new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = ConfigReader.GetSetting<int>("WCF_MaxConcurrentCalls", 100),
                MaxConcurrentInstances = ConfigReader.GetSetting<int>("WCF_MaxConcurrentInstances", 100),
                MaxConcurrentSessions = ConfigReader.GetSetting<int>("WCF_MaxConcurrentSessions", 100)
            });
        }
    }
}
