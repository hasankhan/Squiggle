using System;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;

namespace Messenger
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
            ((Squiggle.UI.MainWindow)app.MainWindow).RestoreWindow();
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
        }

        private void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                RunInBackground = e.Args[0].Trim() == "/background";
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Squiggle.UI.Properties.Settings.Default.Save();
            Environment.Exit(0);
        }
    }
}
