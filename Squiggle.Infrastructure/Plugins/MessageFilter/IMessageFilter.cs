using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Plugins.MessageFilter
{
    public interface IMessageFilter
    {
        FilterDirection Direction { get; }
        bool Filter(StringBuilder message, IChatWindow window);
    }
}
