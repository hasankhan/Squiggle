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

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        SettingsViewModel settingsVm = new SettingsViewModel();
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
            LoadGeneralSettings();
            LoadPersonalSettings();
            LoadConnectionSettings();
            this.DataContext = settingsVm;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            SaveGeneralSettings();
            SavePersonalSettings();
            SaveConnectionSettings();
            this.DialogResult = true;
            Close();
        }

        private void LoadConnectionSettings()
        {
            settingsVm.ConnectionSettings.AllIPs.AddRange(NetworkUtility.GetLocalIPAddresses().Select(ip => ip.ToString()));
            settingsVm.ConnectionSettings.BindToIP = Properties.Settings.Default.BindToIP;
            if (String.IsNullOrEmpty(settingsVm.ConnectionSettings.BindToIP))
                settingsVm.ConnectionSettings.BindToIP = settingsVm.ConnectionSettings.AllIPs.FirstOrDefault();
            settingsVm.ConnectionSettings.ChatPort = Properties.Settings.Default.ChatPort;
            settingsVm.ConnectionSettings.KeepAliveTime = Properties.Settings.Default.KeepAliveTime;
            settingsVm.ConnectionSettings.PresencePort = Properties.Settings.Default.PresencePort;            
        }

        private void LoadGeneralSettings()
        {
            settingsVm.GeneralSettings.HideToSystemTray = Properties.Settings.Default.HideToTray;
            settingsVm.GeneralSettings.ShowPopups = Properties.Settings.Default.ShowPopups;
            settingsVm.GeneralSettings.RunAtStartup = GetRunAtStartup();
        }

        private void LoadPersonalSettings()
        {
            settingsVm.PersonalSettings.DisplayName = user.DisplayName;
            settingsVm.PersonalSettings.DisplayMessage = user.DisplayMessage;
            settingsVm.PersonalSettings.IdleTimeout = Properties.Settings.Default.IdelTimeout;
        }

        private void SavePersonalSettings()
        {
            user.DisplayName = settingsVm.PersonalSettings.DisplayName;
            user.DisplayMessage = settingsVm.PersonalSettings.DisplayMessage;
            Properties.Settings.Default.IdelTimeout = settingsVm.PersonalSettings.IdleTimeout;
        }

        private void SaveConnectionSettings()
        {
            Properties.Settings.Default.BindToIP = settingsVm.ConnectionSettings.BindToIP;
            Properties.Settings.Default.ChatPort = settingsVm.ConnectionSettings.ChatPort;
            Properties.Settings.Default.KeepAliveTime = settingsVm.ConnectionSettings.KeepAliveTime;
            Properties.Settings.Default.PresencePort = settingsVm.ConnectionSettings.PresencePort;
        }

        private void SaveGeneralSettings()
        {
            Properties.Settings.Default.HideToTray = settingsVm.GeneralSettings.HideToSystemTray;
            Properties.Settings.Default.ShowPopups = settingsVm.GeneralSettings.ShowPopups;
            SetRunAtStartup(settingsVm.GeneralSettings.RunAtStartup);
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
