using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Plugins.Authentication
{
    public interface IAuthenticationProvider
    {
        AuthenticationResult Authenticate(string username, string password);
    }
}
