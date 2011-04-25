using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Settings
{
    class ContactSettings
    {
        public string ContactListSortField { get; set; }
        public ContactGroups ContactGroups { get; set; }
        public bool GroupContacts { get; set; }
        public bool ShowOfflineContatcs { get; set; }

        public ContactSettings()
        {
            ContactListSortField = "DisplayName";
            GroupContacts = true;
            ContactGroups = new ContactGroups();
        }
    }
}
