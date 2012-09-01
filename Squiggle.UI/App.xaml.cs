using System;
using System.Diagnostics;
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
            // First time app is launched
            app = new App();
            // You have	to add this	line to	the	MSDN sample
            // to make it work here...
            app.InitializeComponent();
            app.Run();

            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            // Subsequent launches
            base.OnStartupNextInstance(eventArgs);
            ((Squiggle.UI.Windows.MainWindow)app.MainWindow).RestoreWindow();
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
            AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
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
            Async.UIScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Squiggle.UI.Properties.Settings.Default.Save();
            Environment.Exit(0);
        }
    }
}
