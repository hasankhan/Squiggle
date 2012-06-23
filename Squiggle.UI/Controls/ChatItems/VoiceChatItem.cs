using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Chat;
using System.Windows.Documents;
using Squiggle.Core.Chat.Voice;

namespace Squiggle.UI.Controls.ChatItems
{
    class VoiceChatItem: UIChatItem<VoiceChatControl>
    {
        public IVoiceChat Session { get; private set; }
        public string BuddyName { get; private set; }
        public bool Sending { get; private set; }
        public bool AlreadyInChat { get; private set; }

        public VoiceChatItem(IVoiceChat session, string buddyName, bool sending, bool alreadyInChat)
        {
            this.Session = session;
            this.BuddyName = buddyName;
            this.Sending = sending;
            this.AlreadyInChat = alreadyInChat;
        }

        protected override VoiceChatControl CreateControl()
        {
            var control = new VoiceChatControl(Session, BuddyName, Sending, AlreadyInChat);
            return control;
        }
    }
}
