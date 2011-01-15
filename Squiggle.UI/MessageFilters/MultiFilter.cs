using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.MessageFilters
{
    class MultiFilter: List<IMessageFilter>, IMessageFilter
    {
        public bool Filter(StringBuilder message, ChatWindow window)
        {
            foreach (IMessageFilter filter in this)
                if (!filter.Filter(message, window))
                    return false;

            return true;
        }
    }
}
