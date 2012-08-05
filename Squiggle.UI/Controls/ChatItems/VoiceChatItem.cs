using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Squiggle.Chat;
using Squiggle.Activities;
using Squiggle.UI.Components;

namespace Squiggle.UI.Controls.ChatItems
{
    class VoiceChatItem: UIChatItem<VoiceChatControl>
    {
        SquiggleContext context;

        public IVoiceChatHandler Session { get; private set; }
        public string BuddyName { get; private set; }
        public bool Sending { get; private set; }
        public bool AlreadyInChat { get; private set; }

        public VoiceChatItem(SquiggleContext context, IVoiceChatHandler session, string buddyName, bool sending, bool alreadyInChat)
        {
            this.Session = session;
            this.BuddyName = buddyName;
            this.Sending = sending;
            this.AlreadyInChat = alreadyInChat;
            this.context = context;
        }

        protected override VoiceChatControl CreateControl()
        {
            var control = new VoiceChatControl(context, Session, BuddyName, Sending, AlreadyInChat);
            return control;
        }
    }
}
