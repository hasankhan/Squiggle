using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Chat
{
    public static class ClientExtensions
    {
        public static bool IsVoiceChatActive(this IChatClient client)
        {
            return client.ActiveVoiceChat != null;
        }
    }
}
