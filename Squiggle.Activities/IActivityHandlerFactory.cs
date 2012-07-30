using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat;

namespace Squiggle.Activities
{
    public interface IActivityHandlerFactory
    {
        Guid ActivityId { get; }
        IActivityHandler FromInvite(ActivitySession session, IDictionary<string, string> metadata);
        IActivityHandler CreateInvite(ActivitySession session, IDictionary<string, object> args);
    }
}
