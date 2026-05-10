using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
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
            var services = new ServiceCollection();
            services.AddSingleton<BridgeConfiguration>(_ => BridgeConfiguration.GetConfig());
            services.AddTransient<SquiggleBridgeService>();
            var provider = services.BuildServiceProvider();

            ConsoleService.Run(() => provider.GetRequiredService<SquiggleBridgeService>(), args);
		}
	}
}
