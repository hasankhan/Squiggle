using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.UI.Settings
{
    class ConnectionSettings
    {
        public int PresencePort { get; set; }
        public int ChatPort { get; set; }
        public int KeepAliveTime { get; set; }
        public string BindToIP { get; set; }
        public string PresenceAddress { get; set; }

        public ConnectionSettings()
        {
            PresencePort = 9998;
            ChatPort = 9999;
            KeepAliveTime = 10;
            PresenceAddress = "224.10.11.12";
        }
    }
}
