using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using Squiggle.Core;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Utilities.Net;

namespace Squiggle.Multicast
{
    public partial class SquiggleMulticastService : ConsoleService
    {
        MulticastService service;

        public SquiggleMulticastService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            IPEndPoint endPoint = GetConfig();

            DumpConfig(endPoint);

            service = new MulticastService(endPoint);
            service.Start();
        }

        void DumpConfig(IPEndPoint endPoint)
        {
            Trace.WriteLine(":: Settings ::");
            Trace.WriteLine("");
            Trace.WriteLine("Endpoint: " + endPoint);
            Trace.WriteLine("");            
        }

        static IPEndPoint GetConfig()
        {
            string temp = ConfigurationManager.AppSettings["ip"];
            IPAddress ip;
            if (String.IsNullOrEmpty(temp) || !IPAddress.TryParse(temp, out ip) || !NetworkUtility.IsValidLocalIP(ip))
                ip = NetworkUtility.GetLocalIPAddress();

            var reader = new ConfigReader();
            int port = reader.GetSetting<int>("port", DefaultValues.PresencePort);

            var endPoint = new IPEndPoint(ip, port);
            return endPoint;
        }

        protected override void OnStop()
        {
            service.Stop();
            service = null;
        }
    }
}
