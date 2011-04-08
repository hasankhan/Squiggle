using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace Squiggle.Chat
{
    public static class BindingHelper
    {
        public static Binding CreateBinding()
        {
            var binding = new NetTcpBinding(SecurityMode.None);

            binding.MaxReceivedMessageSize = 65535;
            binding.ReaderQuotas.MaxArrayLength = 65535;
            binding.MaxConnections = 100;

            return binding;
        }
    }
}
