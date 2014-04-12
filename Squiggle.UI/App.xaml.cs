using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using Squiggle.UI.Resources;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;

namespace Squiggle.UI
{
    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();

            // This code allows that MEF components define their own configuration sections.
            // Apparently, MEF does not load the assemblies in a way that the AppDomain 
            // likes and it does not registers the load, ... or something.
            // This event is fired whenever some code asks for an assembly that is not in
            // AppDomain's knowledge.
            // more info at http://stackoverflow.com/questions/4845801/custom-configuration-sections-in-mef-exporting-assemblies
            // and at http://msdn.microsoft.com/en-us/library/system.appdomain.assemblyresolve.aspx
            AppDomain.CurrentDomain.AssemblyResolve += (o, a) =>
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                return loadedAssemblies.FirstOrDefault(asm => asm.FullName == a.Name);
            };

            manager.Run(args);
        }
    }

    // Using VB	bits to	detect single instances	and	process	accordingly:
    //	* OnStartup	is fired when the first	instance loads
    //	* OnStartupNextInstance	is fired when the application is re-run	again
    //	  NOTE:	it is redirected to	this instance thanks to	IsSingleInstance
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        App app;

        public SingleInstanceManager()
        {

#if !DEBUG
            this.IsSingleInstance = true;
#endif
        }

        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            app = new App();
            app.InitializeComponent();
            app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            ((Squiggle.UI.Windows.MainWindow)app.MainWindow).Restore();
        }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool RunInBackground { get; set; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            Trace.WriteLine("App domain unhandled exception: " + e.ExceptionObject.ToString());
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
#if !DEBUG
            e.Handled = true;
            Trace.WriteLine("Dispatcher unhandled exception: " + e.Exception.ToString());
#endif
        }

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                RunInBackground = e.Args[0].Trim() == "/background";

            Translation.Initialize();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Squiggle.UI.Properties.Settings.Default.Save();
            Environment.Exit(0);
        }
    }
}
