using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Squiggle.Utilities
{
    public class ConsoleService : ServiceBase
    {
        protected ILogger Logger { get; }

        public ConsoleService() : this(null) { }

        public ConsoleService(ILogger? logger)
        {
            Logger = logger ?? NullLogger.Instance;
        }

        public void RunConsole(string[] args)
        {
            OnStart(args);
            Logger.LogInformation("{ServiceName} running... Press any key to stop", ServiceName);
            Console.ReadKey();
            OnStop();
        }

        public static void Run<TService>(string[] args) where TService : ConsoleService, new()
        {
            Run(() => new TService(), args);
        }

        public static void Run(Func<ConsoleService> factory, string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (Environment.UserInteractive)
            {
                try
                {
                    string option = args.Length > 0 ? args[0].ToUpperInvariant() : String.Empty;
                    switch (option)
                    {
                        case "-I":
                        case "/I":
                            Console.WriteLine("Use 'sc.exe create <ServiceName> binPath=<ExePath>' to install as a Windows service.");
                            break;
                        case "-U":
                        case "/U":
                            Console.WriteLine("Use 'sc.exe delete <ServiceName>' to uninstall the Windows service.");
                            break;
                        default:
                            factory().RunConsole(args);
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
                ServiceBase[] servicesToRun = new ServiceBase[] { factory() };
                ServiceBase.Run(servicesToRun);
            }
        }

        static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ExceptionMonster.Logger?.LogCritical(ex, "Unhandled exception");
        }
    }
}
