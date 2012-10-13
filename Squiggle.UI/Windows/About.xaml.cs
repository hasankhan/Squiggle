using System.Globalization;
using System.Reflection;
using System.Windows;
using Squiggle.UI.Helpers;
using Squiggle.UI.StickyWindow;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;
using Squiggle.UI.Components;
using System;

namespace Squiggle.UI.Windows
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : StickyWindowBase
    {
        public About()
        {
            InitializeComponent();            

            txtVersion.Text = "Version " + AppInfo.Version.ToString();
            string hash = AppInfo.Hash.Substring(0, Math.Min(AppInfo.Hash.Length, 12));
            lnkGitHash.NavigateUri = new Uri("http://squiggle.codeplex.com/SourceControl/changeset/" + hash);
            lnkGitHash.Inlines.Add(hash);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Shell.OpenUrl(e.Uri.ToString());
        }
    }
}
