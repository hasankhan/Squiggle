using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Client;
using Squiggle.Core.Presence;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;

namespace Squiggle.UI.Components
{
    class IdleStatusChanger: IDisposable
    {
        IChatClient client;
        UserStatus? lastStatus;
        UserActivityMonitor monitor;

        public IdleStatusChanger(IChatClient chatClient, TimeSpan timeout)
        {
            this.client = chatClient;
            this.monitor = new UserActivityMonitor(timeout);
            monitor.Idle += (sender, e) =>
            {
                if (client.IsLoggedIn && client.CurrentUser.Status.In(UserStatus.Online, UserStatus.Busy))
                {
                    lastStatus = client.CurrentUser.Status;
                    client.CurrentUser.Status = UserStatus.Idle;
                }
            };
            monitor.Active += (sender, e) =>
            {
                if (client.IsLoggedIn && lastStatus.HasValue)
                    client.CurrentUser.Status = lastStatus.Value;
                lastStatus = null;
            };
            monitor.Start();
        }

        public void Dispose()
        {
            monitor.Dispose();
        }
    }
}
