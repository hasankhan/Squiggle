using System;
using System.Threading;
using System.Windows.Threading;
using Squiggle.UI.Helpers;
using Squiggle.Utilities;
using Squiggle.Utilities.Threading;

namespace Squiggle.UI.Components
{
    class NetworkSinginInfo
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
    }

    class NetworkSignout: IDisposable
    {
        bool autoSignout;
        
        string userName;
        string groupName;

        Action<NetworkSinginInfo> signinFunction;
        Action signoutFunction;
        bool loggedIn;
        Dispatcher dispatcher;

        public NetworkSignout(Dispatcher dispatcher, Action<NetworkSinginInfo> signinFunction, Action signoutFunction)
        {
            this.dispatcher = dispatcher;
            this.signinFunction = signinFunction;
            this.signoutFunction = signoutFunction;
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += new System.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }

        public void OnSignIn(string userName, string groupName)
        {
            this.userName = userName;
            this.groupName = groupName;
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
                    if (autoSignout && !String.IsNullOrEmpty(userName) && !loggedIn)
                        signinFunction(new NetworkSinginInfo() { DisplayName = userName, GroupName = groupName });
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
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged -= new System.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }
    }
}
