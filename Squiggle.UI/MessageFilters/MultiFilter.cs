using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.UI.Windows;
using Squiggle.Plugins.MessageFilter;
using Squiggle.Plugins;

namespace Squiggle.UI.MessageFilters
{
    class MultiFilter: List<IMessageFilter>
    {
        public void Filter(string message, IChatWindow window, FilterDirection direction, Action<string> onAccept)
        {
            var filteredMessage = new StringBuilder(message);
            if (Filter(filteredMessage, window, direction))
                onAccept(filteredMessage.ToString());
        }

        public bool Filter(StringBuilder message, IChatWindow window, FilterDirection direction)
        {
            foreach (IMessageFilter filter in this.Where(f=>(f.Direction & direction) == direction))
                if (!filter.Filter(message, window))
                    return false;

            return true;
        }
    }
}
