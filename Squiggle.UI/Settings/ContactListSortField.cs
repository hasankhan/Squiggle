using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Utilities;

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
