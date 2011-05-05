using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Squiggle.Chat;
using Squiggle.UI.Settings;
using Squiggle.UI.Helpers;
using Squiggle.UI.ViewModel;
using System.Globalization;
using Squiggle.Utilities;
using Squiggle.UI.StickyWindows;
using System.Net;
using Squiggle.UI.Resources;
using Squiggle.Chat.Services;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : StickyWindow
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
            if (SaveSettings())
            {
                this.DialogResult = true;
                Close();
            }           
        }

        void LoadSettings()
        {
            SettingsProvider.Current.Load();
            settingsVm = new SettingsViewModel(SettingsProvider.Current.Settings);
            settingsVm.ConnectionSettings.AllIPs.AddRange(NetworkUtility.GetLocalIPAddresses().Select(ip => ip.ToString()));
            settingsVm.GeneralSettings.RunAtStartup = GetRunAtStartup();
            cmbSortField.SelectedItem = settingsVm.ContactSettings.ContactListSortField;
            cmbContactsView.SelectedItem = settingsVm.ContactSettings.ContactListView;

            if (user == null)
            {
                settingsVm.PersonalSettings.DisplayName = SettingsProvider.Current.Settings.PersonalSettings.DisplayName;
                settingsVm.PersonalSettings.GroupName = SettingsProvider.Current.Settings.PersonalSettings.GroupName;
                settingsVm.PersonalSettings.DisplayMessage = SettingsProvider.Current.Settings.PersonalSettings.DisplayMessage;
                settingsVm.PersonalSettings.EmailAddress = SettingsProvider.Current.Settings.PersonalSettings.EmailAddress;
            }
            else
            {
                settingsVm.PersonalSettings.DisplayName = user.DisplayName;
                settingsVm.PersonalSettings.GroupName = user.Properties.GroupName;
                settingsVm.PersonalSettings.DisplayMessage = user.Properties.DisplayMessage;
                settingsVm.PersonalSettings.EmailAddress = user.Properties.EmailAddress;
            }
        }

        bool SaveSettings()
        {
            IPAddress presenceAddress;
            if (!IPAddress.TryParse(settingsVm.ConnectionSettings.PresenceAddress, out presenceAddress))
            {
                MessageBox.Show(Translation.Instance.SettingsWindow_Error_InvalidPresenceIP, Translation.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            settingsVm.ContactSettings.ContactListSortField = (ContactListSortField)cmbSortField.SelectedItem;
            settingsVm.ContactSettings.ContactListView = (ContactListView)cmbContactsView.SelectedItem;
            settingsVm.Update();

            if (user != null)
            {
                user.DisplayName = settingsVm.PersonalSettings.DisplayName;
                user.Properties.DisplayMessage = settingsVm.PersonalSettings.DisplayMessage;
                user.Properties.EmailAddress = settingsVm.PersonalSettings.EmailAddress;
            }

            TrayPopup.Instance.Enabled = settingsVm.GeneralSettings.ShowPopups;
            AudioAlert.Instance.Enabled = settingsVm.GeneralSettings.AudioAlerts;

            SetRunAtStartup(settingsVm.GeneralSettings.RunAtStartup);
            SettingsProvider.Current.Save();

            return true;
        }

        private bool GetRunAtStartup()
        {
            bool run = ExceptionMonster.EatTheException(()=>
                {
                    return WinStartup.IsAdded("squiggle", GetStartupPath());
                }, "reading startup info from registry");
            return run;
        }

        private void SetRunAtStartup(bool run)
        {
            Exception ex;
            if (!ExceptionMonster.EatTheException(() =>
                {
                    var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    if (run)
                        WinStartup.Add("squiggle", GetStartupPath());
                    else
                        WinStartup.Remove("squiggle");
                }, "setting squiggle startup option in registry", out ex))
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

        private void btnPresenceAddressReset_Click(object sender, RoutedEventArgs e)
        {
            settingsVm.ConnectionSettings.PresenceAddress = DefaultValues.PresenceAddress;
        }

        static string GetStartupPath()
        {
            return AppInfo.FilePath + " /background";
        }        
    }
}
