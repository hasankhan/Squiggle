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
using System.Threading;

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

        public static void ShowFontDialog()
        {
            using (var dialog = new System.Windows.Forms.FontDialog())
            {
                var settings = SettingsProvider.Current.Settings.PersonalSettings;
                dialog.Font = new System.Drawing.Font(settings.FontName, settings.FontSize, settings.FontStyle);
                dialog.ShowColor = true;

                dialog.Color = settings.FontColor;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    float fontSize = dialog.Font.Size;
                    System.Drawing.Color fontColor = dialog.Color;

                    settings.FontColor = fontColor;
                    settings.FontName = dialog.Font.Name;
                    settings.FontSize = Convert.ToInt32(fontSize);
                    settings.FontStyle = dialog.Font.Style;
                    SettingsProvider.Current.Save();
                }
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

        public static void ShakeWindow(ChatWindow window)
        {            
            if (window.WindowState != System.Windows.WindowState.Minimized)
            {
                var rand = new Random();
                double top = window.Top;
                double left = window.Left;

                for (int i = 0; i < 10; i++)
                {
                    window.Top = top + rand.Next(-100, 100);
                    window.Left = left + rand.Next(-100, 100);
                    Thread.Sleep(10);                    
                }

                window.Top = top;
                window.Left = left;
            }
        }

        public static FontSetting GetFontSettings()
        {
            var settings = SettingsProvider.Current.Settings.PersonalSettings;
            var fontSettings = new FontSetting(settings.FontColor, settings.FontName, settings.FontSize, settings.FontStyle);

            return fontSettings;
        }
    }
}
