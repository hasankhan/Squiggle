using System;
using System.Threading;
using System.Windows.Threading;
using Squiggle.UI.Helpers;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;
using Squiggle.Plugins;
using System.Threading.Tasks;

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

        async void NetworkChange_NetworkAvailabilityChanged(object sender, System.Net.NetworkInformation.NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                await Task.Delay(10.Seconds());
                if (autoSignout && signInOptions != null && !loggedIn)
                    this.dispatcher.Invoke(() => signInFunction(signInOptions));
            }
            else
            {
                this.dispatcher.Invoke(() => signoutFunction());
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
