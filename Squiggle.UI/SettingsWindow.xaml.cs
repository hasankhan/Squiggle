using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Squiggle.Chat;
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
            cmbSortField.Text = settingsVm.GeneralSettings.ContactListSortField;
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

            if (user == null)
            {
                settingsVm.PersonalSettings.DisplayName = SettingsProvider.Current.Settings.PersonalSettings.DisplayName;
                settingsVm.PersonalSettings.DisplayMessage = SettingsProvider.Current.Settings.PersonalSettings.DisplayMessage;
            }
            else
            {
                settingsVm.PersonalSettings.DisplayName = user.DisplayName;
                settingsVm.PersonalSettings.DisplayMessage = user.DisplayMessage;
            }
        }

        void SaveSettings()
        {
            settingsVm.GeneralSettings.ContactListSortField = cmbSortField.Text;
            settingsVm.Update();

            if (user != null)
            {
                user.DisplayName = settingsVm.PersonalSettings.DisplayName;
                user.DisplayMessage = settingsVm.PersonalSettings.DisplayMessage;
            }
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

        private void btnBrowseDownloadsFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = settingsVm.GeneralSettings.DownloadsFolder;
            dialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtDownloadsFolder.Text = settingsVm.GeneralSettings.DownloadsFolder = dialog.SelectedPath;
        }        
    }
}
