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

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class ConfirmationDialog : Window
    {
        public ConfirmationDialog()
        {
            InitializeComponent();
        }

        public static bool Show(string username, ConfirmationDialogType dialogType, Window parent)
        {
            if (Properties.Settings.Default.ConfirmDialogs == null)
            {
                Properties.Settings.Default.ConfirmDialogs = new System.Collections.Generic.List<string>();
                Properties.Settings.Default.Save();
            }

            string key = username + "_" + dialogType.Code + ":";

            bool? answer = GetAnswer(key);
            if (answer.HasValue)
                return answer.Value;
            else
            {
                var dialog = new ConfirmationDialog();
                dialog.Title = dialogType.Title;
                dialog.Owner = parent;
                dialog.message.Content = dialogType.Message;
                bool result = dialog.ShowDialog().GetValueOrDefault();
                if (dialog.chkDoNotShow.IsChecked.GetValueOrDefault())
                    SaveAnswer(key, result);
                return result;
            }
        }

        private static bool? GetAnswer(string key)
        {
            string item = Properties.Settings.Default.ConfirmDialogs.FirstOrDefault(x => x.StartsWith(key));
            if (item == null)
                return null;
            else
                return Boolean.Parse(item.Split(':')[1]);
        }

        private static void SaveAnswer(string key, bool result)
        {
            Properties.Settings.Default.ConfirmDialogs.Add(key + result.ToString());
            Properties.Settings.Default.Save();
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
