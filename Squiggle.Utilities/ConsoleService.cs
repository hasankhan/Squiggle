using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Diagnostics;
using System.Configuration.Install;
using System.Reflection;

namespace Squiggle.Utilities
{
    public class ConsoleService : ServiceBase
    {
        public void RunConsole(string[] args)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
            OnStart(args);
            Trace.WriteLine(this.ServiceName + " running... Press any key to stop");
            Trace.WriteLine("");
            Console.ReadKey();
            OnStop();
        }

        public static void Run<TService>(string[] args) where TService : ConsoleService, new()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            if (Environment.UserInteractive)
            {
                try
                {
                    string option = args.Length > 0 ? args[0].ToUpperInvariant() : String.Empty;
                    switch (option)
                    {
                        case "-I":
                        case "/I":
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            break;
                        case "-U":
                        case "/U":
                            ManagedInstallerClass.InstallHelper(new string[] { "/U", Assembly.GetExecutingAssembly().Location });
                            break;
                        default:
                            new TService().RunConsole(args);
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
                ServiceBase[] servicesToRun = new ServiceBase[] { new TService() };
                ServiceBase.Run(servicesToRun);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
                Trace.WriteLine(((Exception)e.ExceptionObject).Message);
        }
    }
}
