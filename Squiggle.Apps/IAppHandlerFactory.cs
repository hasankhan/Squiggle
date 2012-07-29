using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat;

namespace Squiggle.Apps
{
    public interface IAppHandlerFactory
    {
        Guid AppId { get; }
        IAppHandler FromInvite(AppSession session, IDictionary<string, string> metadata);
        IAppHandler CreateInvite(AppSession session, IDictionary<string, object> args);
    }
}
