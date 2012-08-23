using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat.Activity
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
