using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Squiggle.Chat;
using Squiggle.UI.Settings;
using Squiggle.UI.ViewModel;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for BuddyList.xaml
    /// </summary>
    public partial class ContactListControl : UserControl
    {
        public event EventHandler<ChatStartEventArgs> ChatStart = delegate { };
        public event EventHandler SignOut = delegate { };
        public event EventHandler OpenAbout = delegate { };

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
            var cvs = (CollectionViewSource)this.FindResource("buddiesCollection");
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
            OpenAbout(this, EventArgs.Empty);
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
            var cvs = (CollectionViewSource)this.FindResource("buddiesCollection");
            ConfigureCollectionView(cvs);
        }

        void AddGroupDescription(CollectionViewSource cvs)
        {
            var group = new PropertyGroupDescription("Properties.GroupName", null, StringComparison.InvariantCultureIgnoreCase);
            cvs.GroupDescriptions.Add(group);
        }

        void AddSortDescription(CollectionViewSource cvs)
        {
            var sort = new SortDescription();
            sort.PropertyName = SettingsProvider.Current.Settings.GeneralSettings.ContactListSortField;
            cvs.SortDescriptions.Add(sort);         
        }

        void Current_SettingsUpdated(object sender, EventArgs e)
        {
            var cvs = (CollectionViewSource)this.FindResource("buddiesCollection");

            bool refresh = ConfigureCollectionView(cvs);
            
            if (refresh)
                Refresh();
        }

        bool ConfigureCollectionView(CollectionViewSource cvs)
        {
            bool refresh = false;
            if (!cvs.SortDescriptions.Any() ||
                (cvs.SortDescriptions[0].PropertyName != SettingsProvider.Current.Settings.GeneralSettings.ContactListSortField))
            {
                cvs.SortDescriptions.Clear();
                AddSortDescription(cvs);
                refresh = true;
            }
            if (cvs.GroupDescriptions.Any() ^ SettingsProvider.Current.Settings.GeneralSettings.GroupContacts)
            {
                cvs.GroupDescriptions.Clear();
                if (SettingsProvider.Current.Settings.GeneralSettings.GroupContacts)
                    AddGroupDescription(cvs);
                refresh = true;
            }
            return refresh;
        }

        private void Group_ExpandChanged(object sender, RoutedEventArgs e)
        {
            var expander = (Expander)sender;
            ContactGroup group = SettingsProvider.Current.Settings.GeneralSettings.ContactGroups.FirstOrDefault(g => g.GroupName.Equals(expander.Tag));
            if (group != null)
            {
                group.Expanded = expander.IsExpanded;
                SettingsProvider.Current.Save();
            }
        }

        private void Group_Loaded(object sender, RoutedEventArgs e)
        {
            var expander = (Expander)sender;
            ContactGroup group = SettingsProvider.Current.Settings.GeneralSettings.ContactGroups.FirstOrDefault(g => g.GroupName.Equals(expander.Tag));
            expander.IsExpanded = group != null ? group.Expanded : true;
        }
    }

    public class ChatStartEventArgs : EventArgs
    {
        public Buddy User { get; set; }
        public bool SendFile { get; set; }
        public string File { get; set; }
    }
}
