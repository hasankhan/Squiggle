using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.MessageFilters
{
    [Flags]
    public enum FilterDirection
    {
        In = 1,
        Out = 2,
        InOut = In | Out
    }

    public interface IMessageFilter
    {
        FilterDirection Direction { get; }
        bool Filter(StringBuilder message, ChatWindow window);
    }
}
