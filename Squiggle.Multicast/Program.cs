using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Squiggle.Utilities;

namespace Squiggle.Multicast
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            ServiceHelper.Run<SquiggleMulticastService>(args);
        }
    }
}
