using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat;
using Squiggle.Core.Chat.Activity;
using Squiggle.Activities;

namespace Squiggle.Plugins.Activity
{
    public interface IActivity
    {
        Guid Id { get; }
        string Title { get; }
        IActivityHandler FromInvite(IActivityExecutor executor, IDictionary<string, string> metadata);
        IDictionary<string, object> LaunchInviteUI();
        IActivityHandler CreateInvite(IActivityExecutor executor, IDictionary<string, object> args);
    }
}
