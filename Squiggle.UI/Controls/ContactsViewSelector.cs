using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Squiggle.UI.Settings;

namespace Squiggle.UI.Controls
{
    public class ContactsViewSelector : DataTemplateSelector
    {
        public DataTemplate StandardView { get; set; }
        public DataTemplate CompactView { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(SettingsProvider.Current.Settings.ContactSettings.ContactListView == ContactListView.Compact)
                return CompactView;

            return StandardView;
        }
    }
}
