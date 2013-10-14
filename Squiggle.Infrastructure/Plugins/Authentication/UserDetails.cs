using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Plugins.Authentication
{
    public class UserDetails
    {
        public byte[] Image { get; set; }
        public string DisplayName { get; set; }
        public string DisplayMessage { get; set; }
        public string GroupName { get; set; }
        public string Email { get; set; }
    }
}
