using Squiggle.Client;
using Squiggle.Plugins;
using Squiggle.Plugins.Authentication;
using Squiggle.UI.Components.Authentication;
using Squiggle.UI.Factories;
using Squiggle.UI.Resources;
using Squiggle.UI.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Squiggle.UI.Components
{
    class LoginHelper
    {
        IAuthenticationProvider authenticationProvider;
        SquiggleSettings settings;
        IChatClient client;
        
        public LoginHelper(IAuthenticationProvider authenticationProvider, SquiggleSettings settings, IChatClient client)
        {
            this.authenticationProvider = authenticationProvider;
            this.settings = settings;
            this.client = client;
        }

        public void Login(SignInOptions signInOptions)
        {
            var credential = new NetworkCredential(signInOptions.Username, signInOptions.Password, signInOptions.Domain);

            AuthenticationResult result = authenticationProvider.Authenticate(credential);
            if (result.Status == AuthenticationStatus.Failure)
                throw new AuthenticationException(result.Status, Translation.Instance.Authentication_Failed);
            if (result.Status == AuthenticationStatus.ServiceUnavailable)
                throw new AuthenticationException(result.Status, Translation.Instance.Authentication_ServiceUnavailable);

            var optionsFactory = new ChatClientOptionsFactory(settings, result.UserDetails, signInOptions);
            ChatClientOptions clientOptions = optionsFactory.CreateInstance();

            client.Login(clientOptions);
        }
    }
}
