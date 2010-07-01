using System;
using System.Threading;

namespace Squiggle.UI
{
    class NetworkSignout
    {
        bool autoSignout;
        string userName;
        Action<string> signinFunction;
        Action signoutFunction;
        bool loggedIn;

        public NetworkSignout(Action<string> signinFunction, Action signoutFunction)
        {
            this.signinFunction = signinFunction;
            this.signoutFunction = signoutFunction;
            System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged += new System.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler(NetworkChange_NetworkAvailabilityChanged);
        }

        public void OnSignIn(string userName)
        {
            this.userName = userName;
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
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(5000);
                    if (autoSignout && !String.IsNullOrEmpty(userName) && !loggedIn)
                        signinFunction(userName);
                });
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
