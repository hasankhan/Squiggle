using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat.History.DAL
{
    partial class StatusUpdate
    {
        public UserStatus Status
        {
            get { return (UserStatus)StatusCode; }
            set { StatusCode = (int)value; }
        }
    }
}
