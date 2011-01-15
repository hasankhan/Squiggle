using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.MessageFilters
{
    public interface IMessageFilter
    {
        bool Filter(StringBuilder message);
    }
}
