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

        public GenericActivityChatItem(IActivityHandler session, string activityName, bool sending) : base(session, sending) 
        {
            this.activityName = activityName;
        }

        protected override GenericActivityControl CreateControl()
        {
            return new GenericActivityControl(Session, Sending);
        }
    }
}
