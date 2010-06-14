using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Settings;
using System.IO;
using System.Diagnostics;
using Squiggle.Chat;

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
    }
}
