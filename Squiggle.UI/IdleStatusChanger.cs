using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using Squiggle.Utilities;

namespace Squiggle.UI
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
                if (client.LoggedIn && client.CurrentUser.Status.In(UserStatus.Online, UserStatus.Busy))
                {
                    lastStatus = client.CurrentUser.Status;
                    client.CurrentUser.Status = UserStatus.Idle;
                }
            };
            monitor.Active += (sender, e) =>
            {
                if (client.LoggedIn && lastStatus.HasValue)
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
