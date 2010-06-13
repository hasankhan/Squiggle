using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Squiggle.Bridge
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "/console")
                new SquiggleBridge().RunConsole(args);
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new SquiggleBridge() 
			    };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
