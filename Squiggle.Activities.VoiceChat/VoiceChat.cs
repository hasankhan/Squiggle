using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Squiggle.Core.Chat;
using Squiggle.Plugins.Activity;

namespace Squiggle.Activities.VoiceChat
{
    [Export(typeof(IActivity))]
    public class VoiceChat: IActivity
    {
        public Guid Id
        {
            get { return SquiggleActivities.VoiceChat; }
        }

        public string Title
        {
            get { return "Voice Chat"; }
        }

        public IActivityHandler FromInvite(ActivitySession session, IDictionary<string, string> metadata)
        {
            var invitation = new VoiceChatHandler(session);
            return invitation;
        }

        public IActivityHandler CreateInvite(ActivitySession session, IDictionary<string, object> args)
        {
            var invitation = new VoiceChatHandler(session);
            return invitation;
        }

        public IDictionary<string, object> LaunchInviteUI()
        {
            throw new NotImplementedException();
        }
    }
}
