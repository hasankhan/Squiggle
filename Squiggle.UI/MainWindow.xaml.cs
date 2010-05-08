using System;
using System.Collections.Generic;
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

namespace Messenger
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<User> users = new List<User>()
            {
                new User(){DisplayName="Faisal", Status=Status.Online},
                new User(){DisplayName="Ata", Status=Status.Idle},
                new User(){DisplayName="Hasan", Status=Status.Offline}
            };

            this.Users = users;
            this.DataContext = this;
        }

        public List<User> Users { get; set; }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string displayName = ((TextBlock)sender).Tag.ToString();
            ChatWindow window = new ChatWindow();
            window.Title = displayName;
            window.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //var title = new Bold(new Run("Kashif"));
            //var status = new Run(" (Online)");
            //var displayMessage = new Run("You display message goes here...");
            //txtLoggedInUser.Inlines.Add(title);
            //txtLoggedInUser.Inlines.Add(status);
            //txtLoggedInUser.Inlines.Add(Environment.NewLine);
            //txtLoggedInUser.Inlines.Add(displayMessage);
        }
    }

    public class User
    {
        public string DisplayName { get; set; }
        public Status Status { get; set; }
    }

    public enum Status
    {
        Online,
        Offline,
        Idle
    }
}
