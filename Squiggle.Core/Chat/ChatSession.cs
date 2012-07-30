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

namespace Squiggle.Core.Chat
{
    class ChatSession: IChatSession
    {
        SquiggleEndPoint localUser;
        ChatHost chatHost;
        Dictionary<string, SquiggleEndPoint> remoteUsers;
        bool initialized;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler<AppInivteReceivedEventArgs> AppInviteReceived;
        public event EventHandler SessionEnded = delegate { };
        public event EventHandler GroupChatStarted = delegate { };
        public event EventHandler Initialized = delegate { };

        public Guid ID { get; private set; }
        public IEnumerable<SquiggleEndPoint> RemoteUsers
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

        public ChatSession(Guid sessionID, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser)
        {
            this.ID = sessionID;
            this.chatHost = localHost;
            this.localUser = localUser;

            localHost.ChatInviteReceived += new EventHandler<ChatInviteReceivedEventArgs>(chatHost_ChatInviteReceived);
            localHost.AppInvitationReceived += new EventHandler<AppInvitationReceivedEventArgs>(chatHost_AppInvitationReceived);
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(chatHost_MessageReceived);
            localHost.UserTyping += new EventHandler<SessionEventArgs>(chatHost_UserTyping);
            localHost.BuzzReceived += new EventHandler<SessionEventArgs>(chatHost_BuzzReceived);
            localHost.UserJoined += new EventHandler<SessionEventArgs>(chatHost_UserJoined);
            localHost.UserLeft += new EventHandler<SessionEventArgs>(chatHost_UserLeft);
            localHost.SessionInfoRequested += new EventHandler<SessionEventArgs>(chatHost_SessionInfoRequested);
            localHost.SessionInfoReceived += new EventHandler<SessionInfoEventArgs>(chatHost_SessionInfoReceived);

            remoteUsers = new Dictionary<string, SquiggleEndPoint>();
            CreateRemoteUsers(Enumerable.Repeat(remoteUser, 1));
        }

        public SquiggleEndPoint PrimaryUser
        {
            get 
            { 
                lock (remoteUsers)
                    return remoteUsers.Values.FirstOrDefault(); 
            }
        }

        public void Initialize()
        {
            ExceptionMonster.EatTheException(()=>
            {
                chatHost.GetSessionInfo(ID, localUser, PrimaryUser);                
            },"requesting session info");
        }

        public void SendBuzz()
        {
            BroadCast(endpoint => chatHost.Buzz(ID, localUser, endpoint));
        }

        public void UpdateUser(SquiggleEndPoint user)
        {
            AddRemoteUser(user);
        }

        public void NotifyTyping()
        {
            BroadCast(endpoint => chatHost.UserIsTyping(ID, localUser, endpoint));
        }

        public ActivitySession CreateAppSession()
        {
            if (IsGroupSession)
                throw new InvalidOperationException("Cannot send files in a group chat session.");

            return ActivitySession.Create(ID, chatHost, localUser, PrimaryUser);
        }

        public void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            BroadCast(endpoint => chatHost.ReceiveMessage(ID, localUser, endpoint, fontName, fontSize, color, fontStyle, message));
        }

        public void End()
        {
            chatHost.ChatInviteReceived -= new EventHandler<ChatInviteReceivedEventArgs>(chatHost_ChatInviteReceived);
            chatHost.AppInvitationReceived -= new EventHandler<AppInvitationReceivedEventArgs>(chatHost_AppInvitationReceived);
            chatHost.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(chatHost_MessageReceived);
            chatHost.UserTyping -= new EventHandler<SessionEventArgs>(chatHost_UserTyping);
            chatHost.BuzzReceived -= new EventHandler<SessionEventArgs>(chatHost_BuzzReceived);
            chatHost.UserJoined -= new EventHandler<SessionEventArgs>(chatHost_UserJoined);
            chatHost.UserLeft -= new EventHandler<SessionEventArgs>(chatHost_UserLeft);
            chatHost.SessionInfoRequested -= new EventHandler<SessionEventArgs>(chatHost_SessionInfoRequested);
            chatHost.SessionInfoReceived -= new EventHandler<SessionInfoEventArgs>(chatHost_SessionInfoReceived);

            ExceptionMonster.EatTheException(() =>
            {
                BroadCast(endpoint => chatHost.LeaveChat(ID, localUser, endpoint));
            }, "sending leave message");
            SessionEnded(this, EventArgs.Empty);
        }

        public void Invite(SquiggleEndPoint user)
        {
            chatHost.ReceiveChatInvite(ID, localUser, user, RemoteUsers);
        }

        void chatHost_SessionInfoRequested(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID)
                Async.Invoke(() =>
                {
                    ExceptionMonster.EatTheException(() =>
                    {
                        var participants = RemoteUsers.Except(Enumerable.Repeat(e.Sender, 1)).ToArray();
                        chatHost.ReceiveSessionInfo(ID, localUser, e.Sender, participants);
                    }, "sending session info");
                });
        }

        void chatHost_SessionInfoReceived(object sender, SessionInfoEventArgs e)
        {
            if (e.SessionID == ID && e.Participants != null)
            {
                bool wasGroupSession = IsGroupSession;
                CreateRemoteUsers(e.Participants);
                if (!wasGroupSession && IsGroupSession)
                    GroupChatStarted(this, EventArgs.Empty);

                if (!initialized)
                    Initialized(this, EventArgs.Empty);
                initialized = true;
            }
        }

        void chatHost_UserLeft(object sender, SessionEventArgs e)
        {
            bool left = false;
            if (e.SessionID == ID && IsGroupSession)
                left = remoteUsers.Remove(e.Sender.ClientID);

            if (left)
                UserLeft(this, e);
        }

        void chatHost_UserJoined(object sender, SessionEventArgs e)
        {
            bool joined = false;
            lock (remoteUsers)
                if (e.SessionID == ID && !remoteUsers.ContainsKey(e.Sender.ClientID))
                {
                    AddRemoteUser(e.Sender);
                    joined = true;
                }
            if (joined)
                UserJoined(this, e);
        }

        void chatHost_ChatInviteReceived(object sender, ChatInviteReceivedEventArgs e)
        {
            if (e.SessionID == ID)
            {
                ExceptionMonster.EatTheException(() =>
                {
                    CreateRemoteUsers(e.Participants);
                    BroadCast(endpoint => chatHost.JoinChat(ID, localUser, endpoint));
                }, "responding to chat invite");
                GroupChatStarted(this, EventArgs.Empty);
            }
        }

        void chatHost_AppInvitationReceived(object sender, AppInvitationReceivedEventArgs e)
        {
            if (e.SessionID == ID && !IsGroupSession && IsRemoteUser(e.Sender))
            {
                var session = ActivitySession.FromInvite(e.SessionID, chatHost, localUser, e.Sender, e.AppSessionId);
                AppInviteReceived(this, new AppInivteReceivedEventArgs() { Sender = e.Sender, Session = session, AppId = e.AppId, Metadata = e.Metadata });
            }
        }

        void chatHost_UserTyping(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                UserTyping(this, e);
        }

        void chatHost_BuzzReceived(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                BuzzReceived(this, e);
        }

        void chatHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                MessageReceived(this, e);
        }

        bool IsRemoteUser(SquiggleEndPoint user)
        {
            lock (remoteUsers)
                return remoteUsers.ContainsKey(user.ClientID);
        }

        void CreateRemoteUsers(IEnumerable<SquiggleEndPoint> users)
        {
            foreach (SquiggleEndPoint user in users)
                AddRemoteUser(user);
        }

        void AddRemoteUser(SquiggleEndPoint endpoint)
        {
            lock (remoteUsers)
                remoteUsers[endpoint.ClientID] = endpoint;
        }

        void BroadCast(Action<SquiggleEndPoint> userAction)
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
                return ID.Equals(((ChatSession)obj).ID);
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }        
    }
}
