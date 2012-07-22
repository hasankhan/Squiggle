using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using Squiggle.Utilities.Application;

namespace Squiggle.Utilities.Net.Wcf
{
    public static class WcfConfig
    {
        public static Binding CreateBinding()
        {
            var binding = new NetTcpBinding(SecurityMode.None);

            binding.MaxReceivedMessageSize = 65535;
            binding.ReaderQuotas.MaxArrayLength = 65535;
            binding.ReaderQuotas.MaxStringContentLength = 65535;
            binding.MaxConnections = 100;
            binding.OpenTimeout = TimeSpan.FromSeconds(5);
            binding.CloseTimeout = TimeSpan.FromSeconds(5);

            return binding;
        }

        public static void ConfigureHost(ServiceHost host)
        {
            var reader = new ConfigReader();            
            host.Description.Behaviors.Add(new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = reader.GetSetting<int>("WCF_MaxConcurrentCalls", 100),
                MaxConcurrentInstances = reader.GetSetting<int>("WCF_MaxConcurrentInstances", 100),
                MaxConcurrentSessions = reader.GetSetting<int>("WCF_MaxConcurrentSessions", 100)
            });
        }
    }
}
