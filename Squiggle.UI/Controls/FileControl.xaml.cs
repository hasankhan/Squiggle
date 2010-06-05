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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for FileControl.xaml
    /// </summary>
    public partial class FileControl : UserControl
    {
        public static DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(string), typeof(FileControl), new PropertyMetadata(null));
        public string Status
        {
            get { return GetValue(StatusProperty) as string; }
            set
            {
                SetValue(StatusProperty, value);
            }
        } 

        public FileControl()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFile();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string file = DataContext as string;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "explorer.exe";
            startInfo.Arguments = "/select,\"" + file + "\"";

            Process.Start(startInfo);
        }

        private void OpenFile()
        {
            string file = DataContext as string;
            Process.Start(new ProcessStartInfo(file));
        }
    }
}
