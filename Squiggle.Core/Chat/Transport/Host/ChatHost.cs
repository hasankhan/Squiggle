using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using Squiggle.Core.Chat.Transport.Messages;
using Squiggle.Utilities;
using Squiggle.Utilities.Net.Pipe;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Core.Chat.Transport.Host
{
    
    public class ChatHost: IDisposable
    {
        IPEndPoint endpoint;
        MessagePipe pipe;

        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<ChatInviteReceivedEventArgs> ChatInviteReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<ActivitySessionEventArgs> ActivityInvitationAccepted = delegate { };
        public event EventHandler<ActivitySessionEventArgs> ActivitySessionCancelled = delegate { };
        public event EventHandler<ActivityInvitationReceivedEventArgs> ActivityInvitationReceived = delegate { };
        public event EventHandler<ActivityDataReceivedEventArgs> ActivityDataReceived = delegate { };
        public event EventHandler<UserActivityEventArgs> UserActivity = delegate { };
        public event EventHandler<SessionEventArgs> SessionInfoRequested = delegate { };
        public event EventHandler<SessionInfoEventArgs> SessionInfoReceived = delegate { };

        public ChatHost(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public void Start()
        {
            this.pipe = new MessagePipe(endpoint);
            pipe.MessageReceived += new EventHandler<Utilities.Net.Pipe.MessageReceivedEventArgs>(pipe_MessageReceived);
            pipe.Open();
        }

        void pipe_MessageReceived(object sender, Utilities.Net.Pipe.MessageReceivedEventArgs e)
        {
            SerializationHelper.Deserialize<Message>(e.Message, msg => OnMessageReceived(msg), "chat message");            
        }

        void OnMessageReceived(Message msg)
        {
            if (msg is ActivityCancelMessage)
                CancelActivitySession((ActivityCancelMessage)msg);
            else if (msg is ActivityDataMessage)
                ReceiveActivityData((ActivityDataMessage)msg);
            else if (msg is ActivityInviteAcceptMessage)
                AcceptActivityInvite((ActivityInviteAcceptMessage)msg);
            else if (msg is ActivityInviteMessage)
                ReceiveActivityInvite((ActivityInviteMessage)msg);
            else if (msg is BuzzMessage)
                Buzz((BuzzMessage)msg);
            else if (msg is ChatInviteMessage)
                ReceiveChatInvite((ChatInviteMessage)msg);
            else if (msg is ChatJoinMessage)
                JoinChat((ChatJoinMessage)msg);
            else if (msg is ChatLeaveMessage)
                LeaveChat((ChatLeaveMessage)msg);
            else if (msg is GiveSessionInfoMessage)
                GetSessionInfo((GiveSessionInfoMessage)msg);
            else if (msg is SessionInfoMessage)
                ReceiveSessionInfo((SessionInfoMessage)msg);
            else if (msg is TextMessage)
                ReceiveMessage((TextMessage)msg);
            else if (msg is UserTypingMessage)
                UserIsTyping((UserTypingMessage)msg);
        }

        public void Send(Message message)
        {
            byte[] data = SerializationHelper.Serialize(message);
            pipe.Send(message.Recipient.Address, data);
        }

        void GetSessionInfo(GiveSessionInfoMessage msg)
        {
            SessionInfoRequested(this, new SessionEventArgs(msg.SessionId, msg.Sender));
            Trace.WriteLine(msg.Sender + " is requesting session info.");
        }

        void ReceiveSessionInfo(SessionInfoMessage msg)
        {
            SessionInfoReceived(this, new SessionInfoEventArgs() { Participants = msg.Participants.ToArray(), Sender = msg.Sender, SessionID = msg.SessionId });
            Trace.WriteLine(msg.Sender + " is sent session info.");
        }

        void Buzz(BuzzMessage msg)
        {
            OnUserActivity(msg.SessionId, msg.Sender, ActivityType.Buzz);
            BuzzReceived(this, new SessionEventArgs(msg.SessionId, msg.Sender));
            Trace.WriteLine(msg.Sender + " is buzzing.");
        }

        void UserIsTyping(UserTypingMessage msg)
        {
            OnUserActivity(msg.SessionId, msg.Sender, ActivityType.Typing);
            UserTyping(this, new SessionEventArgs(msg.SessionId, msg.Sender ));
            Trace.WriteLine(msg.Sender + " is typing.");
        }

        void ReceiveMessage(TextMessage msg)
        {
            OnUserActivity(msg.SessionId, msg.Sender, ActivityType.Message);
            MessageReceived(this, new MessageReceivedEventArgs()
            {
                SessionID = msg.SessionId, 
                Sender = msg.Sender,
                FontName = msg.FontName,
                FontSize = msg.FontSize,
                Color = msg.Color,
                FontStyle = msg.FontStyle,
                Message = msg.Message 
            });
            Trace.WriteLine("Message received from: " + msg.Sender + ", sessionId= " + msg.SessionId);
        }

        void ReceiveChatInvite(ChatInviteMessage msg)
        {
            OnUserActivity(msg.SessionId, msg.Sender, ActivityType.ChatInvite);
            Trace.WriteLine(msg.Sender + " invited you to group chat.");
            ChatInviteReceived(this, new ChatInviteReceivedEventArgs() 
            { 
                SessionID = msg.SessionId, 
                Sender = msg.Sender, 
                Participants = msg.Participants.ToArray() 
            });
        }

        void JoinChat(ChatJoinMessage msg)
        {
            Trace.WriteLine(msg.Sender + " has joined the chat.");
            UserJoined(this, new UserActivityEventArgs() { SessionID = msg.SessionId, Sender = msg.Sender});
        }

        void LeaveChat(ChatLeaveMessage msg)
        {
            Trace.WriteLine(msg.Sender + " has left the chat.");
            UserLeft(this, new UserActivityEventArgs() { SessionID = msg.SessionId, Sender = msg.Sender});
        }

        void ReceiveActivityInvite(ActivityInviteMessage msg)
        {
            OnUserActivity(msg.SessionId, msg.Sender, ActivityType.TransferInvite);
            Trace.WriteLine(msg.Sender + " wants to send a file " + msg.Metadata.ToTraceString());
            ActivityInvitationReceived(this, new ActivityInvitationReceivedEventArgs()
            {
                SessionID = msg.SessionId,
                Sender = msg.Sender,
                ActivityId = msg.ActivityId,
                ActivitySessionId = msg.ActivitySessionId,
                Metadata = msg.Metadata.ToDictionary(i => i.Key, i => i.Value)
            });
        }

        void ReceiveActivityData(ActivityDataMessage msg)
        {
            ActivityDataReceived(this, new ActivityDataReceivedEventArgs() { ActivitySessionId = msg.SessionId, Chunk = msg.Data });
        }

        void AcceptActivityInvite(ActivityInviteAcceptMessage msg)
        {
            ActivityInvitationAccepted(this, new ActivitySessionEventArgs() { ActivitySessionId = msg.SessionId });
        }

        void CancelActivitySession(ActivityCancelMessage msg)
        {
            ActivitySessionCancelled(this, new ActivitySessionEventArgs() { ActivitySessionId = msg.SessionId });
        }       

        void OnUserActivity(Guid sessionId, SquiggleEndPoint sender, ActivityType type)
        {
            UserActivity(this, new UserActivityEventArgs(){Sender = sender, SessionID = sessionId, Type = type});
        }

        public void Dispose()
        {
            if (pipe != null)
            {
                pipe.Dispose();
                pipe = null;
            }
        }
    }


    public class SessionEventArgs : EventArgs
    {
        public Guid SessionID { get; set; }
        public SquiggleEndPoint Sender { get; set; }

        public SessionEventArgs(){}

        public SessionEventArgs(Guid sessionId, SquiggleEndPoint user)
        {
            this.SessionID = sessionId;
            this.Sender = user;
        }        
    }

    public class ChatInviteReceivedEventArgs : SessionEventArgs
    {
        public SquiggleEndPoint[] Participants { get; set; }
    }

    public class MessageReceivedEventArgs : SessionEventArgs
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public Color Color { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Message { get; set; }
    }

    public class ActivitySessionEventArgs : EventArgs
    {
        public Guid ActivitySessionId { get; set; }
    }

    public class ActivityInvitationReceivedEventArgs : SessionEventArgs
    {
        public Guid ActivitySessionId { get; set; }
        public Guid ActivityId { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
    }

    public class ActivityDataReceivedEventArgs : ActivitySessionEventArgs
    {
        public byte[] Chunk { get; set; }
    }

    public enum ActivityType
    {
        Message,
        Typing,
        Buzz,
        TransferInvite,
        ChatInvite
    }

    public class UserActivityEventArgs : SessionEventArgs
    {
        public ActivityType Type { get; set; }
    }

    public class SessionInfoEventArgs : SessionEventArgs
    {
        public SquiggleEndPoint[] Participants { get; set; }
    }
}
