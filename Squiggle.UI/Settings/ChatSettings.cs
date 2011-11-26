using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.UI.Settings
{
    class ChatSettings
    {
        public bool EnableLogging { get; set; }
        public bool ShowEmoticons { get; set; }
        public bool SpellCheck { get; set; }
        public bool StealFocusOnNewMessage { get; set; }
        public bool ClearChatOnWindowClose { get; set; }
        public int MaxMessagesToPreserve { get; set; }
    }
}
