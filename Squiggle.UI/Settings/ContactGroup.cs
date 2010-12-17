using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Settings
{
    public class ContactGroup
    {
        public string GroupName { get; set; }
        public DateTime LastUsed { get; set; }

        public override int GetHashCode()
        {
            return GroupName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is ContactGroup)
                return this.GroupName.Equals(((ContactGroup)obj).GroupName);
            return false;
        }
    }
}
