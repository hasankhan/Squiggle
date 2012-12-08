using System;
using System.Threading;
using System.Windows.Threading;
using Squiggle.UI.Helpers;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;
using Squiggle.Plugins;

namespace Squiggle.UI.Components
{
    class NetworkSignout: IDisposable
    {
        bool autoSignout;

        SignInOptions signInOptions;

        Action<SignInOptions> signInFunction;
        Action signoutFunction;
        bool loggedIn;
        Dispatcher dispatcher;

        public NetworkSignout(Dispatcher dispatcher, Action<SignInOptions> signInFunction, Action signoutFunction)
        {
            this.dispatcher = dispatcher;
            this.signInFunction = signInFunction;
            this.signoutFunction = signoutFunction;
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;
        }

        public void OnSignIn(SignInOptions signInOptions)
        {
            this.signInOptions = signInOptions;
            loggedIn = true;
        }

        public void OnSignOut()
        {
            autoSignout = false;
            loggedIn = false;
        }

        void NetworkChange_NetworkAvailabilityChanged(object sender, System.Net.NetworkInformation.NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                dispatcher.Delay(()=>
                {
                    if (autoSignout && signInOptions!=null && !loggedIn)
                        signInFunction(signInOptions);
                }, 10.Seconds());
            }
            else
            {
                signoutFunction();
                if (loggedIn)
                {
                    autoSignout = true;
                    loggedIn = false;
                }
            }
        }

        public void Dispose()
        {
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged -= NetworkChange_NetworkAvailabilityChanged;
        }
    }
}
