using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Diagnostics;

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
            if (args.Length > 0 && args[0] == "/console")
                new SquiggleBridgeService().RunConsole(args);
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
