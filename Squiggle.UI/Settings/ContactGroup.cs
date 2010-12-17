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
        public bool Expanded { get; set; }
    }
}
