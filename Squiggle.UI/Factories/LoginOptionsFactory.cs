using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Client;
using Squiggle.UI.Settings;
using System.Net;
using Squiggle.Utilities;
using Squiggle.UI.Resources;
using Squiggle.Utilities.Net;
using Squiggle.Core;
using Squiggle.Plugins.Authentication;
using Squiggle.Plugins;
using Squiggle.Core.Presence;

namespace Squiggle.UI.Factories
{
    class LoginOptionsFactory: IInstanceFactory<LoginOptions>
    {
        SquiggleSettings settings;
        UserDetails userInfo;
        SignInOptions signInOptions;

        public LoginOptionsFactory(SquiggleSettings settings, UserDetails userInfo, SignInOptions signInOptions)
        {
            this.settings = settings;
            this.userInfo = userInfo;
            this.signInOptions = signInOptions;
        }

        public LoginOptions CreateInstance()
        {
            int chatPort = settings.ConnectionSettings.ChatPort;
            if (String.IsNullOrEmpty(settings.ConnectionSettings.BindToIP))
                throw new OperationCanceledException(Translation.Instance.Error_NoNetwork);

            var localIP = IPAddress.Parse(settings.ConnectionSettings.BindToIP);
            TimeSpan keepAliveTimeout = settings.ConnectionSettings.KeepAliveTime.Seconds();

            IPAddress presenceAddress;
            if (!NetworkUtility.TryParseAddress(settings.ConnectionSettings.PresenceAddress, out presenceAddress))
                throw new ApplicationException(Translation.Instance.SettingsWindow_Error_InvalidPresenceIP);

            var chatEndPoint = NetworkUtility.GetFreeEndPoint(new IPEndPoint(localIP, chatPort));
            var presenceServiceEndPoint = NetworkUtility.GetFreeEndPoint(new IPEndPoint(localIP, settings.ConnectionSettings.PresencePort));
            var multicastEndPoint = new IPEndPoint(presenceAddress, settings.ConnectionSettings.PresencePort);
            var multicastReceiveEndPoint = NetworkUtility.GetFreeEndPoint(new IPEndPoint(localIP, settings.ConnectionSettings.PresenceCallbackPort));

            string clientID = settings.ConnectionSettings.ClientID;

            string displayName = Environment.ExpandEnvironmentVariables(userInfo.DisplayName.NullIfEmpty() ?? 
                                                                        signInOptions.DisplayName.NullIfEmpty() ?? 
                                                                        settings.PersonalSettings.DisplayName);
            var options = new LoginOptions()
            {
                ChatEndPoint = chatEndPoint,
                MulticastEndPoint = multicastEndPoint,
                MulticastReceiveEndPoint = multicastReceiveEndPoint,
                PresenceServiceEndPoint = presenceServiceEndPoint,
                KeepAliveTime = keepAliveTimeout,
                UserProperties = CreateProperties(),
                DisplayName = displayName
            };

            return options;
        }

        IBuddyProperties CreateProperties()
        {
            string groupName = Environment.ExpandEnvironmentVariables(userInfo.GroupName.NullIfEmpty() ?? 
                                                                       signInOptions.GroupName.NullIfEmpty() ?? 
                                                                       settings.PersonalSettings.GroupName);

            string email = Environment.ExpandEnvironmentVariables(userInfo.Email.NullIfEmpty() ?? settings.PersonalSettings.EmailAddress);

            string displayMessage = Environment.ExpandEnvironmentVariables(settings.PersonalSettings.DisplayMessage);

            var properties = new BuddyProperties();
            properties.GroupName = groupName;
            properties.EmailAddress = email;
            properties.DisplayImage = userInfo.Image ?? settings.PersonalSettings.DisplayImage;
            properties.DisplayMessage = displayMessage;
            properties.MachineName = Environment.MachineName;

            return properties;
        }
    }
}
