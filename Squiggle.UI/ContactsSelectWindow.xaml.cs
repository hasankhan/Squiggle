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
using Squiggle.Chat;
using System.Collections.ObjectModel;
using Squiggle.UI.Controls;

namespace Squiggle.UI
{
    /// <summary>
    /// Interaction logic for ContactsWindow.xaml
    /// </summary>
    public partial class ContactsSelectWindow : Window
    {
        private ClientViewModel clientViewModel;
        private List<Buddy> selectedContacts;
        string filter = string.Empty;

        public ReadOnlyCollection<Buddy> SelectedContacts
        {
            get { return new ReadOnlyCollection<Buddy>(selectedContacts); }
        }

        public static DependencyProperty AllowMultiSelectProperty = DependencyProperty.Register("AllowMultiSelect", typeof(bool?), typeof(ContactsSelectWindow), new PropertyMetadata(null));
        public bool? AllowMultiSelect
        {
            get { return GetValue(AllowMultiSelectProperty) as bool?; }
            set
            {
                SetValue(AllowMultiSelectProperty, value);
            }
        } 

        public ContactsSelectWindow()
        {
            InitializeComponent();
        }

        public ContactsSelectWindow(ClientViewModel clientViewModel, bool allowMultiSelect) : this()
        {
            this.clientViewModel = clientViewModel;
            this.AllowMultiSelect = allowMultiSelect;
            if (allowMultiSelect)
                txtMessage.Text = "To send an instant message, select one or more contacts";

            this.clientViewModel.BuddyOffline += new EventHandler(clientViewModel_BuddyOffline);

            selectedContacts = new List<Buddy>();
            this.DataContext = this.clientViewModel;
        }

        void clientViewModel_BuddyOffline(object sender, EventArgs e)
        {
            Refresh();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            CloseDialog(false);
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            CloseDialog(true);
        }

        private void CloseDialog(bool result)
        {
            this.DialogResult = result;
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var buddy = ((CheckBox)sender).Tag as Buddy;
            selectedContacts.Add(buddy);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var buddy = ((CheckBox)sender).Tag as Buddy;
            if(selectedContacts.Contains(buddy))
                selectedContacts.Remove(buddy);
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            Buddy buddy = (Buddy)e.Item;
            if (buddy.Status == UserStatus.Offline)
                e.Accepted = false;

            else if (filter == String.Empty)
                e.Accepted = true;
            else
                e.Accepted = buddy.DisplayName.ToLower().Contains(filter.ToLower());
        }

        private void FilterTextBox_FilterChanged(object sender, BuddyFilterEventArs e)
        {
            filter = e.FilterBy;
            Refresh();
        }

        void Refresh()
        {
            CollectionViewSource cvs = (CollectionViewSource)this.FindResource("buddiesCollection");
            if (cvs.View != null)
                cvs.View.Refresh();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseDialog(false);
        }  

        private void contactBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var parent = sender as Border;
            if (AllowMultiSelect.HasValue && AllowMultiSelect.Value)
            {
                var selection = parent.FindName("chkSelection") as CheckBox;

                selection.IsChecked = !selection.IsChecked;
            }
            else
            {
                selectedContacts.Clear();
                selectedContacts.Add(parent.Tag as Buddy);

                CloseDialog(true);
            }
        }

    }
}
