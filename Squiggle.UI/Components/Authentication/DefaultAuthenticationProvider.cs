using Squiggle.Plugins.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Components.Authentication
{
    class DefaultAuthenticationProvider: IAuthenticationProvider
    {
        public bool RequiresUsername
        {
            get { return false; }
        }

        public bool RequiresPassword
        {
            get { return false; }
        }

        public bool RequiresDomain
        {
            get { return false; }
        }

        public bool ReturnsGroupName
        {
            get { return false; }
        }

        public bool ReturnsDisplayName
        {
            get { return false; }
        }

        public AuthenticationResult Authenticate(System.Net.NetworkCredential credential)
        {
            return new AuthenticationResult(AuthenticationStatus.Success);
        }
    }
}
