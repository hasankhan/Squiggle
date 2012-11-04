using Squiggle.Core.Chat.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Controls.ChatItems.Activity
{
    class GenericActivityChatItem: ActivityChatItem<GenericActivityControl, IActivityHandler>
    {
        string activityName;
        string buddyName;

        public GenericActivityChatItem(IActivityHandler session, string activityName, string buddyName, bool sending) : base(session, sending) 
        {
            this.activityName = activityName;
            this.buddyName = buddyName;
        }

        protected override GenericActivityControl CreateControl()
        {
            return new GenericActivityControl(Session, activityName, buddyName, Sending);
        }
    }
}
