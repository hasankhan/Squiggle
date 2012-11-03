using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Squiggle.Client;
using Squiggle.Core;
using Squiggle.UI.Helpers;
using Squiggle.UI.Resources;
using Squiggle.UI.Settings;
using Squiggle.UI.StickyWindow;
using Squiggle.UI.ViewModel;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.Utilities.Net;
using Squiggle.UI.Components;

namespace Squiggle.UI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : StickyWindowBase
    {
        SettingsViewModel settingsVm;
        ISelfBuddy buddy;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(ISelfBuddy user) : this()
        {
            this.buddy = user;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();

            if (buddy != null)
                personalTab.IsSelected = true;

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

            if (buddy == null)
            {
                settingsVm.PersonalSettings.DisplayName = SettingsProvider.Current.Settings.PersonalSettings.DisplayName;
                settingsVm.PersonalSettings.GroupName = SettingsProvider.Current.Settings.PersonalSettings.GroupName;
                settingsVm.PersonalSettings.DisplayMessage = SettingsProvider.Current.Settings.PersonalSettings.DisplayMessage;
                settingsVm.PersonalSettings.DisplayImage = SettingsProvider.Current.Settings.PersonalSettings.DisplayImage;
                settingsVm.PersonalSettings.EmailAddress = SettingsProvider.Current.Settings.PersonalSettings.EmailAddress;
            }
            else
            {
                settingsVm.PersonalSettings.DisplayName = buddy.DisplayName;
                settingsVm.PersonalSettings.GroupName = buddy.Properties.GroupName;
                settingsVm.PersonalSettings.DisplayMessage = buddy.Properties.DisplayMessage;
                settingsVm.PersonalSettings.DisplayImage = buddy.Properties.DisplayImage;
                settingsVm.PersonalSettings.EmailAddress = buddy.Properties.EmailAddress;
            }
        }

        bool SaveSettings()
        {
            IPAddress presenceAddress;
            if (!NetworkUtility.TryParseAddress(settingsVm.ConnectionSettings.PresenceAddress, out presenceAddress))
            {
                MessageBox.Show(Translation.Instance.SettingsWindow_Error_InvalidPresenceIP, Translation.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            settingsVm.ContactSettings.ContactListSortField = (ContactListSortField)cmbSortField.SelectedItem;
            settingsVm.ContactSettings.ContactListView = (ContactListView)cmbContactsView.SelectedItem;
            settingsVm.Update();

            if (buddy != null)
            {
                buddy.DisplayName = settingsVm.PersonalSettings.DisplayName;
                buddy.Properties.DisplayMessage = settingsVm.PersonalSettings.DisplayMessage;
                buddy.Properties.DisplayImage = settingsVm.PersonalSettings.DisplayImage;
                buddy.Properties.EmailAddress = settingsVm.PersonalSettings.EmailAddress;
            }

            TrayPopup.Instance.Enabled = settingsVm.GeneralSettings.ShowPopups;
            AudioAlert.Instance.Enabled = settingsVm.GeneralSettings.AudioAlerts;
            SquiggleContext.Current.ChatClient.EnableLogging = settingsVm.GeneralSettings.EnableStatusLogging;

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

        private void btnSetDisplayImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = Translation.Instance.Global_ImageFilter + "|*.jpg;*.jpeg;*.bmp;*.gif";
            if (dialog.ShowDialog(this) == true)
            {
                if (ImageUtility.IsValidImage(dialog.FileName))
                {
                    using (var image = Image.FromFile(dialog.FileName))
                    using (var resized = image.ResizeTo(100, 100))
                        settingsVm.PersonalSettings.DisplayImage = resized.ToBytes();
                }
                else
                    MessageBox.Show(Translation.Instance.Error_InvalidImage, Translation.Instance.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearDisplayImage_Click(object sender, RoutedEventArgs e)
        {
            settingsVm.PersonalSettings.DisplayImage = null;
        }
    }
}
