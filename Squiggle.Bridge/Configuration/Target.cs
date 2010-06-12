using System;
using System.Configuration;

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
    }
}
