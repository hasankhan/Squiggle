using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Squiggle.Client.Activities;
using Squiggle.Core.Chat.Activity;
using Squiggle.Plugins;

namespace Squiggle.VoiceChat
{
    [Export(typeof(IActivity))]
    public class VoiceChatActivity : IActivity
    {
        public Guid Id => SquiggleActivities.VoiceChat;

        public string Title => "Voice Chat";

        public IActivityHandler FromInvite(IActivityExecutor executor, IDictionary<string, string> metadata)
        {
            return new VoiceChatHandler(executor);
        }

        public IActivityHandler CreateInvite(IActivityExecutor executor, IDictionary<string, object> args)
        {
            return new VoiceChatHandler(executor);
        }

        public Task<IDictionary<string, object>> LaunchInviteUI(ISquiggleContext context, IChatWindow window)
        {
            throw new NotImplementedException();
        }
    }
}
