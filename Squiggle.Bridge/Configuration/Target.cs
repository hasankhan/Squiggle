using System;
using System.Configuration;
using System.Net;

namespace Squiggle.Bridge.Configuration
{
    class Target : ConfigurationElement
    {
        [ConfigurationProperty("IP", IsRequired = true)]
        public string IP
        {
            get { return this["IP"] as string; }
        }

        [ConfigurationProperty("Port", IsRequired = true)]
        public int Port
        {
            get { return Convert.ToInt32(this["Port"]); }
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
