using Squiggle.Plugins.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Components.Authentication
{
    class AuthenticationException : Exception
    {
        public AuthenticationStatus Status { get; private set; }

        public AuthenticationException(AuthenticationStatus status, string message)
            : base(message)
        {
            this.Status = status;
        }
    }
}
