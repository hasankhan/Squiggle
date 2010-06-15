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
using Squiggle.Chat;
using System.Reflection;
using Squiggle.UI.Settings;
using System.ComponentModel;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for BuddyList.xaml
    /// </summary>
    public partial class ContactListControl : UserControl
    {
        public event EventHandler<ChatStartEventArgs> ChatStart = delegate { };
        public event EventHandler SignOut = delegate { };

        string filter = String.Empty;

        public static DependencyProperty ChatContextProperty = DependencyProperty.Register("ChatContext", typeof(ClientViewModel), typeof(ContactListControl), new PropertyMetadata(null));
        public ClientViewModel ChatContext
        {
            get { return GetValue(ChatContextProperty) as ClientViewModel; }
            set 
            {
                SetValue(ChatContextProperty, value);
            }
        } 

        public ContactListControl()
        {
            InitializeComponent();

            SettingsProvider.Current.SettingsUpdated += new EventHandler(Current_SettingsUpdated);
        }

        public void Refresh()
        {
            CollectionViewSource cvs = (CollectionViewSource)this.FindResource("buddiesCollection");
            if (cvs.View != null)
                cvs.View.Refresh();
        }

        private void ComboBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SignOut(this, new EventArgs());
            e.Handled = true;
        }

        private void ComboBoxItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SignOut(this, new EventArgs());
                e.Handled = true;
            }
        }

        private void About_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SquiggleUtility.ShowAboutDialog();
        }

        private void Buddy_Click(object sender, MouseButtonEventArgs e)
        {
            Buddy buddy = ((Border)sender).Tag as Buddy;
            StartChat(buddy, false, null);
        }        

        private void StartChat(Buddy buddy, bool sendFile, string filePath)
        {
            if (buddy.Status != UserStatus.Offline)
                ChatStart(this, new ChatStartEventArgs() { User = buddy,
                                                           SendFile = sendFile,
                                                           File = filePath });
        }

        private void StartChat_Click(object sender, RoutedEventArgs e)
        {
            Buddy buddy = ((MenuItem)sender).Tag as Buddy;
            StartChat(buddy, false, null);
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            Buddy buddy = ((MenuItem)sender).Tag as Buddy;
            StartChat(buddy, true, null);
        }

        private void Buddy_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
        }

        private void Buddy_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                Buddy buddy = ((Border)sender).Tag as Buddy;
                StartChat(buddy, true, files.FirstOrDefault());
            }
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            Buddy buddy = (Buddy)e.Item;
            if (filter == String.Empty)
                e.Accepted = true;
            else
                e.Accepted = buddy.DisplayName.ToLower().Contains(filter.ToLower());
        }

        private void FilterTextBox_FilterChanged(object sender, BuddyFilterEventArs e)
        {
            filter = e.FilterBy;

            Refresh();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource cvs = (CollectionViewSource)this.FindResource("buddiesCollection");
            AddSortDescription(cvs);
        }

        private static void AddSortDescription(CollectionViewSource cvs)
        {
            var sd = new SortDescription();
            sd.PropertyName = SettingsProvider.Current.Settings.GeneralSettings.ContactListSortField;
            cvs.SortDescriptions.Add(sd);
        }

        void Current_SettingsUpdated(object sender, EventArgs e)
        {
            CollectionViewSource cvs = (CollectionViewSource)this.FindResource("buddiesCollection");
            if (cvs.SortDescriptions[0].PropertyName != SettingsProvider.Current.Settings.GeneralSettings.ContactListSortField)
            {
                cvs.SortDescriptions.Clear();
                AddSortDescription(cvs);

                Refresh();
            }
        }
    }

    public class ChatStartEventArgs : EventArgs
    {
        public Buddy User { get; set; }
        public bool SendFile { get; set; }
        public string File { get; set; }
    }
}
