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

    public class SessionEventArgs : EventArgs
    {
        public Guid SessionID { get; set; }
        public ISquiggleEndPoint Sender { get; set; }

        public SessionEventArgs() { }

        public SessionEventArgs(Guid sessionId, ISquiggleEndPoint user)
        {
            this.SessionID = sessionId;
            this.Sender = user;
        }
    }

    public class TextMessageReceivedEventArgs : SessionEventArgs
    {
        public Guid Id { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }

    public class TextMessageUpdatedEventArgs : SessionEventArgs
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }

    public interface IChatSession
    {
        event EventHandler<TextMessageReceivedEventArgs> MessageReceived;
        event EventHandler<TextMessageUpdatedEventArgs> MessageUpdated;
        event EventHandler<SessionEventArgs> BuzzReceived;
        event EventHandler<SessionEventArgs> UserTyping;
        event EventHandler<SessionEventArgs> UserJoined;
        event EventHandler<SessionEventArgs> UserLeft;
        event EventHandler<ActivityInivteReceivedEventArgs> ActivityInviteReceived;
        event EventHandler GroupChatStarted;

        IEnumerable<ISquiggleEndPoint> RemoteUsers { get; }
        Guid Id { get; }
        bool IsGroupSession { get; }

        void SendBuzz();
        void SendMessage(Guid id, string fontName, int fontSize, Color color, FontStyle fontStyle, string message);
        void UpdateMessage(Guid id, string message);
        void NotifyTyping();
        IActivityExecutor CreateActivity();
        void End();
        void Invite(ISquiggleEndPoint user);
        void UpdateUser(ISquiggleEndPoint user);
    }
}
