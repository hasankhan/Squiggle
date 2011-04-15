using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
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
            ServiceHelper.Run<SquiggleBridgeService>(args);			
		}
	}
}
