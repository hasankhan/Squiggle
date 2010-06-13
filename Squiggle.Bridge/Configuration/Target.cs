using System;
using System.Configuration;
using System.Net;

namespace Squiggle.Bridge.Configuration
{
    class Target : ConfigurationElement
    {
        [ConfigurationProperty("ip", IsRequired = true)]
        public string IP
        {
            get { return this["ip"] as string; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return Convert.ToInt32(this["port"]); }
        }

        public IPEndPoint EndPoint
        {
            get 
            {
                var ip = IPAddress.Parse(IP);
                return new IPEndPoint(ip, Port); 
            }
        }
    }
}
