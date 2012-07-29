using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;

namespace Squiggle.Core.Chat
{
    public class AppSession
    {
        SquiggleEndPoint localUser;
        SquiggleEndPoint remoteUser;
        Guid chatSessionId;

        internal Guid Id { get; private set; }
        internal ChatHost ChatHost { get; private set; }
        internal bool SelfInitiated { get; private set; }

        private AppSession(Guid chatSessionId, ChatHost chatHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, Guid Id, bool selfInitiated)
        {
            this.chatSessionId = chatSessionId;
            this.ChatHost = chatHost;
            this.localUser = localUser;
            this.remoteUser = remoteUser;
            this.Id = Id;
            this.SelfInitiated = selfInitiated;
        }

        internal static AppSession Create(Guid sessionId, ChatHost chatHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser)
        {
            var session = new AppSession(sessionId, chatHost, localUser, remoteUser, Guid.NewGuid(), true);
            return session;
        }

        internal static AppSession FromInvite(Guid sessionId, ChatHost chatHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser, Guid appSessionId)
        {
            var session = new AppSession(sessionId, chatHost, localUser, remoteUser, appSessionId, false);
            return session;
        }

        internal bool Accept()
        {
            bool success = ExceptionMonster.EatTheException(() =>
            {
                ChatHost.AcceptAppInvite(Id, localUser, remoteUser);
            }, "accepting app invite from " + remoteUser);
            return success;
        }

        internal void Cancel()
        {
            ExceptionMonster.EatTheException(() => ChatHost.CancelAppSession(Id, localUser, remoteUser), "cancelling app session with user" + remoteUser);
        }

        internal void SendData(byte[] chunk, Action<Exception> onError)
        {
            Exception ex;
            if (!ExceptionMonster.EatTheException(() =>
                {
                    ChatHost.ReceiveAppData(Id, localUser, remoteUser, chunk);
                }, "sending data to " + remoteUser.ToString(), out ex))
                onError(ex);
        }

        internal bool SendInvite(Guid appId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            bool success = ExceptionMonster.EatTheException(() => ChatHost.ReceiveAppInvite(chatSessionId, localUser, remoteUser, appId, Id, metadata), "Sending app invite to " + remoteUser.ToString());
            return success;
        }
    }
}
