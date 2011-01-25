using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;

namespace Squiggle.Bridge.Configuration
{
    class ChannelBinding : ConfigurationElement
    {
        [ConfigurationProperty("ip", IsRequired = true)]
        public string IP
        {
            get { return this["ip"] as string; }
        }

        [ConfigurationProperty("mport", IsRequired = true)]
        public int MulticastPort
        {
            get { return Convert.ToInt32(this["mport"]); }
        }

        [ConfigurationProperty("sport", IsRequired = true)]
        public int ServicePort
        {
            get { return Convert.ToInt32(this["sport"]); }
        }

        public IPEndPoint MulticastEndPoint
        {
            get
            {
                var ip = IPAddress.Parse(IP);
                return new IPEndPoint(ip, MulticastPort);
            }
        }
    }
}
