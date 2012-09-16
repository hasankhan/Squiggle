using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Client;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{
    public interface ISelfBuddy: IBuddy
    {
        new string DisplayName { get;  set; }
        new UserStatus Status { get; set; }
    }
}
