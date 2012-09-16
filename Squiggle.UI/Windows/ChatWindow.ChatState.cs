using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Windows
{
    partial class ChatWindow
    {
        class ChatState
        {
            DateTime? lastBuzzSent;
            DateTime? lastBuzzReceived;

            public DateTime? LastMessageReceived { get; private set; }
            public Guid? LastSentMessageId { get; set; }
            public string LastSavedFile { get; set; }
            public string LastSavedFormat { get; set; }
            public bool BuzzPending { get; set; }
            public bool ChatStarted { get; set; }

            public bool CanSendBuzz
            {
                get { return lastBuzzSent == null || DateTime.Now.Subtract(lastBuzzSent.Value).TotalSeconds > 5; }
            }

            public bool CanReceiveBuzz
            {
                get { return lastBuzzReceived == null || DateTime.Now.Subtract(lastBuzzReceived.Value).TotalSeconds > 5; }
            }

            public void MessageReceived()
            {
                LastMessageReceived = DateTime.Now;
            }

            public void BuzzSent()
            {
                lastBuzzSent = DateTime.Now;
            }

            public void BuzzReceived()
            {
                lastBuzzReceived = DateTime.Now;
            }
        }
    }
}
