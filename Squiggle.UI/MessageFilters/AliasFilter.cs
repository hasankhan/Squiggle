using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.MessageFilters
{
    class AliasFilter: IMessageFilter
    {
        public bool Filter(StringBuilder message)
        {
            return true;
        }
    }
}
