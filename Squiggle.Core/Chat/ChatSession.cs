using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;
using Squiggle.Core.Chat.Activity;
using Squiggle.Utilities.Threading;

namespace Squiggle.Core.Chat
{
    class ChatSession: IChatSession
    {
        ISquiggleEndPoint localUser;
        ChatHost chatHost;
        Dictionary<string, ISquiggleEndPoint> remoteUsers;
        ActionQueue eventQueue = new ActionQueue();
        bool initialized;

        public event EventHandler<TextMessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<TextMessageUpdatedEventArgs> MessageUpdated = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler<ActivityInivteReceivedEventArgs> ActivityInviteReceived;
        public event EventHandler SessionEnded = delegate { };
        public event EventHandler GroupChatStarted = delegate { };
        public event EventHandler Initialized = delegate { };

        public Guid Id { get; private set; }
        public IEnumerable<ISquiggleEndPoint> RemoteUsers
        {
            get 
            {
                lock (remoteUsers)
                    return remoteUsers.Values.ToList(); 
            }
        }

        public bool IsGroupSession
        {
            get 
            { 
                lock (remoteUsers)
                    return remoteUsers.Count > 1; 
            }
        }

        public ChatSession(Guid sessionID, ChatHost localHost, ISquiggleEndPoint localUser, ISquiggleEndPoint remoteUser)
        {
            this.Id = sessionID;
            this.chatHost = localHost;
            this.localUser = localUser;

            localHost.ChatInviteReceived += new EventHandler<ChatInviteReceivedEventArgs>(chatHost_ChatInviteReceived);
            localHost.ActivityInvitationReceived += new EventHandler<ActivityInvitationReceivedEventArgs>(chatHost_ActivityInvitationReceived);
            localHost.TextMessageReceived += new EventHandler<TextMessageReceivedEventArgs>(chatHost_MessageReceived);
            localHost.TextMessageUpdated += new EventHandler<TextMessageUpdatedEventArgs>(chatHost_MessageUpdated);
            localHost.UserTyping += new EventHandler<SessionEventArgs>(chatHost_UserTyping);
            localHost.BuzzReceived += new EventHandler<SessionEventArgs>(chatHost_BuzzReceived);
            localHost.UserJoined += new EventHandler<SessionEventArgs>(chatHost_UserJoined);
            localHost.UserLeft += new EventHandler<SessionEventArgs>(chatHost_UserLeft);
            localHost.SessionInfoRequested += new EventHandler<SessionEventArgs>(chatHost_SessionInfoRequested);
            localHost.SessionInfoReceived += new EventHandler<SessionInfoEventArgs>(chatHost_SessionInfoReceived);

            remoteUsers = new Dictionary<string, ISquiggleEndPoint>();
            CreateRemoteUsers(Enumerable.Repeat(remoteUser, 1));
        }

        public ISquiggleEndPoint PrimaryUser
        {
            get 
            { 
                lock (remoteUsers)
                    return remoteUsers.Values.FirstOrDefault(); 
            }
        }

        public void Initialize(bool needSessionInfo)
        {
            if (needSessionInfo)
                ExceptionMonster.EatTheException(() =>
                {
                    chatHost.GetSessionInfo(Id, localUser, PrimaryUser);
                }, "requesting session info");
            else
                OnInitialized();
        }

        public void SendBuzz()
        {
            BroadCast(endpoint => chatHost.Buzz(Id, localUser, endpoint));
        }

        public void UpdateUser(ISquiggleEndPoint user)
        {
            AddRemoteUser(user);
        }

        public void NotifyTyping()
        {
            BroadCast(endpoint => chatHost.UserIsTyping(Id, localUser, endpoint));
        }

        public IActivityExecutor CreateActivity()
        {
            if (IsGroupSession)
                throw new InvalidOperationException("Cannot send files in a group chat session.");

            var session = ActivitySession.Create(Id, chatHost, localUser, PrimaryUser);
            var executor = new ActivityExecutor(session);
            return executor;
        }

        public void SendMessage(Guid messageId, string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            BroadCast(recipient => chatHost.ReceiveMessage(messageId, Id, localUser, recipient, fontName, fontSize, color, fontStyle, message));
        }

        public void UpdateMessage(Guid messageId, string message)
        {
            BroadCast(recipient => chatHost.UpdateMessage(messageId, Id, localUser, recipient, message));
        }

        public void End()
        {
            chatHost.ChatInviteReceived -= new EventHandler<ChatInviteReceivedEventArgs>(chatHost_ChatInviteReceived);
            chatHost.ActivityInvitationReceived -= new EventHandler<ActivityInvitationReceivedEventArgs>(chatHost_ActivityInvitationReceived);
            chatHost.TextMessageReceived -= new EventHandler<TextMessageReceivedEventArgs>(chatHost_MessageReceived);
            chatHost.TextMessageUpdated -= new EventHandler<TextMessageUpdatedEventArgs>(chatHost_MessageUpdated);
            chatHost.UserTyping -= new EventHandler<SessionEventArgs>(chatHost_UserTyping);
            chatHost.BuzzReceived -= new EventHandler<SessionEventArgs>(chatHost_BuzzReceived);
            chatHost.UserJoined -= new EventHandler<SessionEventArgs>(chatHost_UserJoined);
            chatHost.UserLeft -= new EventHandler<SessionEventArgs>(chatHost_UserLeft);
            chatHost.SessionInfoRequested -= new EventHandler<SessionEventArgs>(chatHost_SessionInfoRequested);
            chatHost.SessionInfoReceived -= new EventHandler<SessionInfoEventArgs>(chatHost_SessionInfoReceived);

            ExceptionMonster.EatTheException(() =>
            {
                BroadCast(endpoint => chatHost.LeaveChat(Id, localUser, endpoint));
            }, "sending leave message");
            SessionEnded(this, EventArgs.Empty);
        }

        public void Invite(ISquiggleEndPoint user)
        {
            chatHost.ReceiveChatInvite(Id, localUser, user, RemoteUsers);
        }

        void chatHost_SessionInfoRequested(object sender, SessionEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            Async.Invoke(() =>
            {
                ExceptionMonster.EatTheException(() =>
                {
                    var participants = RemoteUsers.Except(Enumerable.Repeat(e.Sender, 1)).ToArray();
                    chatHost.ReceiveSessionInfo(Id, localUser, e.Sender, participants);
                }, "sending session info");
            });
        }

        void chatHost_SessionInfoReceived(object sender, SessionInfoEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            if (e.Participants != null)
            {
                bool wasGroupSession = IsGroupSession;

                CreateRemoteUsers(e.Participants);
                if (!wasGroupSession && IsGroupSession)
                    GroupChatStarted(this, EventArgs.Empty);

                OnInitialized();
            }
        }

        void chatHost_UserLeft(object sender, SessionEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            eventQueue.Enqueue(() => OnUserLeft(e));
        }

        void chatHost_UserJoined(object sender, SessionEventArgs e)
        {
            if (e.SessionID != Id)
                return;

             eventQueue.Enqueue(() => OnUserJoined(e));
        }

        void chatHost_ChatInviteReceived(object sender, ChatInviteReceivedEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            OnInviteReceived(e);
        }

        void chatHost_ActivityInvitationReceived(object sender, ActivityInvitationReceivedEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            eventQueue.Enqueue(() => OnActivityInvitationReceived(e));
        }

        void chatHost_UserTyping(object sender, SessionEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            eventQueue.Enqueue(() => OnUserTyping(e));
        }

        void chatHost_BuzzReceived(object sender, SessionEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            eventQueue.Enqueue(() => OnBuzzReceived(e));
        }

        void chatHost_MessageReceived(object sender, TextMessageReceivedEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            eventQueue.Enqueue(() => OnMessageReceived(e));
        }

        void chatHost_MessageUpdated(object sender, TextMessageUpdatedEventArgs e)
        {
            if (e.SessionID != Id)
                return;

            eventQueue.Enqueue(() => OnMessageUpdated(e));
        }

        void OnInitialized()
        {
            if (!initialized)
                Initialized(this, EventArgs.Empty);
            initialized = true;

            eventQueue.Open();
        }

        void OnUserLeft(SessionEventArgs e)
        {
            bool left = IsGroupSession && remoteUsers.Remove(e.Sender.ClientID);
            if (left)
                UserLeft(this, e);
        }

        void OnUserJoined(SessionEventArgs e)
        {
            bool joined = false;
            lock (remoteUsers)
                if (!remoteUsers.ContainsKey(e.Sender.ClientID))
                {
                    AddRemoteUser(e.Sender);
                    joined = true;
                }
            if (joined)
                UserJoined(this, e);
        }

        void OnInviteReceived(ChatInviteReceivedEventArgs e)
        {
            CreateRemoteUsers(e.Participants);

            ExceptionMonster.EatTheException(() =>
            {
                BroadCast(endpoint => chatHost.JoinChat(Id, localUser, endpoint));
            }, "responding to chat invite");

            GroupChatStarted(this, EventArgs.Empty);
        }

        void OnActivityInvitationReceived(ActivityInvitationReceivedEventArgs e)
        {
            if (!IsGroupSession && IsRemoteUser(e.Sender))
            {
                var session = ActivitySession.FromInvite(e.SessionID, chatHost, localUser, e.Sender, e.ActivitySessionId);
                var executor = new ActivityExecutor(session);
                ActivityInviteReceived(this, new ActivityInivteReceivedEventArgs() { Sender = e.Sender, Executor = executor, ActivityId = e.ActivityId, Metadata = e.Metadata });
            }
        }

        void OnUserTyping(SessionEventArgs e)
        {
            if (IsRemoteUser(e.Sender))
                UserTyping(this, e);
        }

        void OnBuzzReceived(SessionEventArgs e)
        {
            if (IsRemoteUser(e.Sender))
                BuzzReceived(this, e);
        }

        void OnMessageReceived(TextMessageReceivedEventArgs e)
        {
            if (IsRemoteUser(e.Sender))
                MessageReceived(this, e);
        }

        void OnMessageUpdated(TextMessageUpdatedEventArgs e)
        {
            if (IsRemoteUser(e.Sender))
                MessageUpdated(this, e);
        }

        bool IsRemoteUser(ISquiggleEndPoint user)
        {
            lock (remoteUsers)
                return remoteUsers.ContainsKey(user.ClientID);
        }

        void CreateRemoteUsers(IEnumerable<ISquiggleEndPoint> users)
        {
            foreach (ISquiggleEndPoint user in users)
                AddRemoteUser(user);
        }

        void AddRemoteUser(ISquiggleEndPoint endpoint)
        {
            lock (remoteUsers)
                remoteUsers[endpoint.ClientID] = endpoint;
        }

        void BroadCast(Action<ISquiggleEndPoint> userAction)
        {
            bool allSuccess = true;

            Parallel.ForEach(RemoteUsers, user =>
            {
                Exception ex;
                if (!ExceptionMonster.EatTheException(() =>
                {
                    userAction(user);
                }, "doing a broadcast operation in chat", out ex))
                {
                    allSuccess = false;
                    Trace.WriteLine(ex.Message);
                }
            });

            if (!allSuccess)
                throw new OperationFailedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is ChatSession)
                return Id.Equals(((ChatSession)obj).Id);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }        
    }
}
