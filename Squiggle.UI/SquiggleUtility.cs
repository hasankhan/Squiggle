using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Settings;
using System.IO;
using System.Diagnostics;
using Squiggle.Chat;
using System.Reflection;
using System.Windows;

namespace Squiggle.UI
{
    class SquiggleUtility
    {
        public static IEnumerable<UserStatus> GetChangableStatuses()
        {
            var statuses = from status in Enum.GetValues(typeof(UserStatus)).Cast<UserStatus>()
                           where status != UserStatus.Idle
                           select status;
            return statuses;
        }

        public static void OpenDownloadsFolder()
        {
            string downloadsFolder = SettingsProvider.Current.Settings.GeneralSettings.DownloadsFolder;
            try
            {
                if (!Directory.Exists(downloadsFolder))
                    Directory.CreateDirectory(downloadsFolder);

                Process.Start(downloadsFolder);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public static void ShowSettingsDialog(Window owner)
        {
            Buddy user = null;
            if (MainWindow.Instance.chatControl.ContactList.ChatContext != null)
                user = MainWindow.Instance.chatControl.ContactList.ChatContext.LoggedInUser;
            var settings = new SettingsWindow(user);
            settings.Owner = owner;
            if (settings.ShowDialog() == true)
            {
                if (MainWindow.Instance.chatControl.SignIn.Visibility == Visibility.Visible)
                    MainWindow.Instance.chatControl.SignIn.txtdisplayName.Text = SettingsProvider.Current.Settings.PersonalSettings.DisplayName;
            }
        }

        public static void ShowAboutDialog()
        {
            string about = @"Squiggle Messenger {0}

Programmed by:
Faisal Iqbal
M. Hasan Khan

Contact:       info@overroot.com
Website:       www.overroot.com";
            about = String.Format(about, Assembly.GetExecutingAssembly().GetName().Version.ToString(3));
            MessageBox.Show(about, "About Squiggle Messenger", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
