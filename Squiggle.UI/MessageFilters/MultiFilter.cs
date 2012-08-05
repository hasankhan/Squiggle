using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Windows;
using Squiggle.UI.Plugins.MessageFilter;
using Squiggle.UI.Plugins;

namespace Squiggle.UI.MessageFilters
{
    class MultiFilter: List<IMessageFilter>
    {
        public bool Filter(StringBuilder message, IChatWindow window, FilterDirection direction)
        {
            foreach (IMessageFilter filter in this.Where(f=>(f.Direction & direction) == direction))
                if (!filter.Filter(message, window))
                    return false;

            return true;
        }
    }
}
