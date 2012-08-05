using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Squiggle.Chat;
using Squiggle.Utilities;
using Squiggle.Utilities.Net;
using Squiggle.UI.Windows;
using System.ComponentModel.Composition;

namespace Squiggle.UI.MessageFilters
{
    [Export(typeof(IMessageFilter))]
    class AliasFilter: IMessageFilter
    {
        public FilterDirection Direction
        {
            get { return FilterDirection.Out; }
        }

        public bool Filter(StringBuilder message, ChatWindow window)
        {
            message.Replace("(you)", window.PrimaryBuddy.DisplayName)
                   .Replace("(me)", MainWindow.Instance.ChatClient.CurrentUser.DisplayName)
                   .Replace("(now)", DateTime.Now.ToString())
                   .Replace("(time)", DateTime.Now.ToLongTimeString())
                   .Replace("(stime)", DateTime.Now.ToShortTimeString())
                   .Replace("(date)", DateTime.Now.ToShortDateString())
                   .Replace("(day)", DateTime.Now.DayOfWeek.ToString())
                   .Replace("(myip)", (NetworkUtility.GetLocalIPAddress() ?? IPAddress.Loopback).ToString())
                   .Replace("(ver)", AppInfo.Version.ToString());
            
            return true;
        }
    }
}
