using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat;

namespace Squiggle.UI.Plugins.Activity
{
    public interface IActivity
    {
        Guid Id { get; }
        string Title { get; }
        IActivityHandler FromInvite(ActivitySession session, IDictionary<string, string> metadata);
        IDictionary<string, object> LaunchInviteUI();
        IActivityHandler CreateInvite(ActivitySession session, IDictionary<string, object> args);
    }
}
