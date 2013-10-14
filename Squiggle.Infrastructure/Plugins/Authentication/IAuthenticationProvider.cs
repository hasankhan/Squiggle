using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Squiggle.Plugins.Authentication
{
    public interface IAuthenticationProvider
    {
        bool RequiresUsername { get; }
        bool RequiresPassword { get; }
        bool RequiresDomain { get; }

        bool ReturnsDisplayName { get; }
        bool ReturnsGroupName { get; }

        AuthenticationResult Authenticate(NetworkCredential credential);
    }
}
