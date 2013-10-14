using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;
using Squiggle.Plugins;
using System.Threading.Tasks;

namespace Squiggle.VoiceChat
{
    [Export(typeof(IActivity))]
    public class VoiceChatActivity: IActivity
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

        public Task<IDictionary<string, object>> LaunchInviteUI(ISquiggleContext context, IChatWindow window)
        {
            throw new NotImplementedException();
        }
    }
}
