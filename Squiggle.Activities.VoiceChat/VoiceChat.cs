using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Squiggle.Chat.Activities;

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

        public IActivityHandler FromInvite(IActivityExecutor executor, IDictionary<string, string> metadata)
        {
            var invitation = new VoiceChatHandler(executor);
            return invitation;
        }

        public IActivityHandler CreateInvite(IActivityExecutor executor, IDictionary<string, object> args)
        {
            var invitation = new VoiceChatHandler(executor);
            return invitation;
        }

        public IDictionary<string, object> LaunchInviteUI()
        {
            throw new NotImplementedException();
        }
    }
}
