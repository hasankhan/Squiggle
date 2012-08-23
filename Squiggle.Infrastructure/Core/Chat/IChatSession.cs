using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using Squiggle.Core.Chat.Activity;

namespace Squiggle.Core.Chat
{
    public class ActivityInivteReceivedEventArgs : EventArgs
    {
        public ISquiggleEndPoint Sender { get; set; }
        public Guid ActivityId { get; set; }
        public IActivityExecutor Executor {get; set;}
        public IDictionary<string, string> Metadata { get; set; }
    }

    public interface IChatSession
    {
        event EventHandler<TextMessageReceivedEventArgs> MessageReceived;
        event EventHandler<SessionEventArgs> BuzzReceived;
        event EventHandler<SessionEventArgs> UserTyping;
        event EventHandler<SessionEventArgs> UserJoined;
        event EventHandler<SessionEventArgs> UserLeft;
        event EventHandler<ActivityInivteReceivedEventArgs> ActivityInviteReceived;
        event EventHandler GroupChatStarted;

        IEnumerable<ISquiggleEndPoint> RemoteUsers { get; }
        Guid ID { get; }
        bool IsGroupSession { get; }

        void SendBuzz();
        void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message);
        void NotifyTyping();
        IActivityExecutor CreateActivity();
        void End();
        void Invite(ISquiggleEndPoint user);
        void UpdateUser(ISquiggleEndPoint user);
    }
}
