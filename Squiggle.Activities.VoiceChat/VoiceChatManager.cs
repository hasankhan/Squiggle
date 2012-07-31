using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Squiggle.Core.Chat;

namespace Squiggle.Activities.VoiceChat
{
    [Export(typeof(IActivityManager))]
    public class VoiceChatManager: IActivityManager
    {
        public Guid ActivityId
        {
            get { return SquiggleActivities.VoiceChat; }
        }

        public string Title
        {
            get { return "Voice Chat"; }
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

        public IDictionary<string, object> LaunchInviteUI()
        {
            throw new NotImplementedException();
        }
    }
}
