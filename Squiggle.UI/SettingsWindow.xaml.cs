using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Squiggle.Chat;
using Microsoft.Win32;
using System.Reflection;
using Squiggle.UI.Settings;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        SettingsViewModel settingsVm;
        Buddy user;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(Buddy user) : this()
        {
            this.user = user;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();

            this.DataContext = settingsVm;
        }        

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.DialogResult = true;
            Close();
        }

        void LoadSettings()
        {
            SettingsProvider.Current.Load();
            settingsVm = new SettingsViewModel(SettingsProvider.Current.Settings);
            settingsVm.ConnectionSettings.AllIPs.AddRange(NetworkUtility.GetLocalIPAddresses().Select(ip => ip.ToString()));
            settingsVm.GeneralSettings.RunAtStartup = GetRunAtStartup();
            settingsVm.PersonalSettings.DisplayName = user.DisplayName;
            settingsVm.PersonalSettings.DisplayMessage = user.DisplayMessage;
        }

        void SaveSettings()
        {
            settingsVm.Update();
            user.DisplayName = settingsVm.PersonalSettings.DisplayName;
            user.DisplayMessage = settingsVm.PersonalSettings.DisplayMessage;
            SetRunAtStartup(settingsVm.GeneralSettings.RunAtStartup);

            SettingsProvider.Current.Save();
        }

        private bool GetRunAtStartup()
        {
            try
            {
                bool run = WinStartup.IsAdded("squiggle");                
                return run;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SetRunAtStartup(bool run)
        {
            try
            {
                var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (run)
                    WinStartup.Add("squiggle", Assembly.GetExecutingAssembly().Location + " /background");
                else
                    WinStartup.Remove("squiggle");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not set Squiggle to run at startup due to exception: " + ex.Message, "Squiggle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }        
    }
}
