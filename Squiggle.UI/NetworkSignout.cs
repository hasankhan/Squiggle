using System;
using System.Threading;
using Squiggle.UI.Helpers;

namespace Squiggle.UI
{
    class NetworkSinginInfo
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
    }

    class NetworkSignout
    {
        bool autoSignout;
        
        string userName;
        string groupName;

        Action<NetworkSinginInfo> signinFunction;
        Action signoutFunction;
        bool loggedIn;

        public NetworkSignout(Action<NetworkSinginInfo> signinFunction, Action signoutFunction)
        {
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
                Async.Invoke(()=>
                {
                    if (autoSignout && !String.IsNullOrEmpty(userName) && !loggedIn)
                        signinFunction(new NetworkSinginInfo() { DisplayName = userName, GroupName = groupName });
                }, TimeSpan.FromSeconds(10));
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
    }
}
