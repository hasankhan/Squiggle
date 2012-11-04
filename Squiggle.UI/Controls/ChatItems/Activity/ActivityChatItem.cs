using Squiggle.Core.Chat.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Squiggle.UI.Controls.ChatItems.Activity
{
    abstract class ActivityChatItem<TControl, TSession> : UIChatItem<TControl> 
        where TControl:Control
        where TSession:IActivityHandler
    {
        public bool Sending { get; private set; }
        public TSession Session { get; private set; }

        protected ActivityChatItem(TSession session, bool sending)
        {
            this.Session = session;
            this.Sending = sending;
        }
    }
}
