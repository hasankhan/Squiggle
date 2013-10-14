using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squiggle.Plugins;

namespace Squiggle.Core.Chat.Activity
{
    public interface IActivity
    {
        Guid Id { get; }
        string Title { get; }
        IActivityHandler FromInvite(IActivityExecutor executor, IDictionary<string, string> metadata);
        Task<IDictionary<string, object>> LaunchInviteUI(ISquiggleContext context, IChatWindow window);
        IActivityHandler CreateInvite(IActivityExecutor executor, IDictionary<string, object> args);
    }
}
