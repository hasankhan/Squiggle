using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Utilities.Net;

namespace Squiggle.Multicast
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/multicast-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            services.AddSingleton<IPEndPoint>(_ => GetEndPoint());
            services.AddTransient<SquiggleMulticastService>();
            var provider = services.BuildServiceProvider();

            ExceptionMonster.Logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("ExceptionMonster");

            ConsoleService.Run(() => provider.GetRequiredService<SquiggleMulticastService>(), args);
        }

        static IPEndPoint GetEndPoint()
        {
            string? temp = System.Configuration.ConfigurationManager.AppSettings["ip"];
            IPAddress ip;
            if (String.IsNullOrEmpty(temp) ||
                !IPAddress.TryParse(temp, out ip) ||
                !NetworkUtility.IsValidLocalIP(ip))
            {
                ip = NetworkUtility.GetLocalIPAddress();
            }

            var reader = new ConfigReader();
            int port = reader.GetSetting<int>("port", Squiggle.Core.DefaultValues.PresencePort);

            return new IPEndPoint(ip, port);
        }
    }
}
