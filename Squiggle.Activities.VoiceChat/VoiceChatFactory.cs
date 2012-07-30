using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Squiggle.Core.Chat;

namespace Squiggle.Activities.VoiceChat
{
    [Export(typeof(IActivityHandlerFactory))]
    public class VoiceChatFactory: IActivityHandlerFactory
    {
        public Guid ActivityId
        {
            get { return SquiggleActivities.VoiceChat; }
        }

        public IActivityHandler FromInvite(ActivitySession session, IDictionary<string, string> metadata)
        {
            var invitation = new VoiceChat(session);
            return invitation;
        }

        public IActivityHandler CreateInvite(ActivitySession session, IDictionary<string, object> args)
        {
            var invitation = new VoiceChat(session);
            return invitation;
        }
    }
}
