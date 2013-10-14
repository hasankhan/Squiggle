using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Plugins.MessageFilter
{
    [Flags]
    public enum FilterDirection
    {
        In = 1,
        Out = 2,
        InOut = In | Out
    }
}
