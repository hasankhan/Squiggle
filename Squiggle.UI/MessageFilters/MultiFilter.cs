using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.MessageFilters
{
    class MultiFilter: List<IMessageFilter>
    {
        public bool Filter(StringBuilder message, ChatWindow window, FilterDirection direction)
        {
            foreach (IMessageFilter filter in this.Where(f=>(f.Direction & direction) == direction))
                if (!filter.Filter(message, window))
                    return false;

            return true;
        }
    }
}
