using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Squiggle.Plugins.Authentication
{
    public interface IAuthenticationProvider
    {
        bool RequiresUsername { get; set; }
        bool RequiresPassword { get; set; }
        bool RequiresDomain { get; set; }
        bool RequiresGroupName { get; set; }

        AuthenticationResult Authenticate(NetworkCredential credential);
    }
}
