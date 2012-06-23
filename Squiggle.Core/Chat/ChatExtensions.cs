using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Squiggle.Core.Chat
{
    public static class ChatExtensions
    {
        public static bool HasVoiceChat(this IChatSession session)
        {
            return session.AppSessions.Any(s => s.AppId == ChatApps.VoiceChat);
        }
    }
}
