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
        public Guid AppId
        {
            get { return SquiggleActivities.VoiceChat; }
        }

        public IAppHandler FromInvite(AppSession session, IDictionary<string, string> metadata)
        {
            var invitation = new VoiceChat(session);
            return invitation;
        }

        public IAppHandler CreateInvite(AppSession session, IDictionary<string, object> args)
        {
            var invitation = new VoiceChat(session);
            return invitation;
        }
    }
}
