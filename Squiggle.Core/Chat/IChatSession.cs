using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using Squiggle.Core.Chat.Transport.Host;

namespace Squiggle.Core.Chat
{
    public class AppInivteReceivedEventArgs: EventArgs
    {
        public SquiggleEndPoint Sender { get; set; }
        public Guid AppId { get; set; }
        public AppSession Session {get; set;}
        public IDictionary<string, string> Metadata { get; set; }
    }

    public interface IChatSession
    {
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler<SessionEventArgs> BuzzReceived;
        event EventHandler<SessionEventArgs> UserTyping;
        event EventHandler<SessionEventArgs> UserJoined;
        event EventHandler<SessionEventArgs> UserLeft;
        event EventHandler<AppInivteReceivedEventArgs> AppInviteReceived;
        event EventHandler GroupChatStarted;

        IEnumerable<SquiggleEndPoint> RemoteUsers { get; }
        Guid ID { get; }
        bool IsGroupSession { get; }

        void SendBuzz();
        void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message);
        void NotifyTyping();
        AppSession CreateAppSession();
        void End();
        void Invite(SquiggleEndPoint user);
        void UpdateUser(SquiggleEndPoint user);
    }
}
