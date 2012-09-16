using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Client;

namespace Squiggle.Client
{
    public static class ClientExtensions
    {
        public static bool IsOnline(this IBuddy buddy)
        {
            return buddy.Status != Core.Presence.UserStatus.Offline;
        }
    }
}
