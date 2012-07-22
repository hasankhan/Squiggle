using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities;
using Squiggle.Utilities.Application;

namespace Squiggle.UI.Settings
{
    public enum ContactListSortField
    {
        [StringValue("SortField_DisplayName")]
        DisplayName,
        [StringValue("SortField_Status")]
        Status
    }
}
