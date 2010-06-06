using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Messenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static bool RunInBackground { get; set; }

        public App()
        {
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
                RunInBackground = e.Args[0].Trim() == "/background";
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Squiggle.UI.Properties.Settings.Default.Save();
        }
    }
}
