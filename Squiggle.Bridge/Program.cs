using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;
using System.Configuration.Install;
using System.Reflection;

namespace Squiggle.Bridge
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
			if (Environment.UserInteractive)
			{
				try
				{
					string option = args[0].ToLower();
					switch (option)
					{
						case "-i":
						case "/i":
							ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
							break;
						case "-u":
						case "/u":
							ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
							break;
						default:
							new SquiggleBridgeService().RunConsole(args);
							break;
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(ex.Message);
				}
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new SquiggleBridgeService() 
				};
				ServiceBase.Run(ServicesToRun);
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception)
				Trace.WriteLine(((Exception)e.ExceptionObject).Message);
		}
	}
}
