using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Plugins.Authentication
{
    public interface IAuthenticationProvider
    {
        AuthenticationResult Authenticate(string username, string password);
    }
}
