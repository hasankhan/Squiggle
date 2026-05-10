using System;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Squiggle.Bridge.Configuration;
using Squiggle.Utilities;

namespace Squiggle.Bridge
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
                .WriteTo.File("logs/bridge-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            services.AddSingleton<BridgeConfiguration>(_ => BridgeConfiguration.GetConfig());
            services.AddTransient<SquiggleBridgeService>();
            var provider = services.BuildServiceProvider();

            ExceptionMonster.Logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("ExceptionMonster");

            ConsoleService.Run(() => provider.GetRequiredService<SquiggleBridgeService>(), args);
		}
	}
}
