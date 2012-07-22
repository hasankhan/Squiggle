using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Squiggle.Core.Chat.FileTransfer;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Core.Chat.Voice;
using Squiggle.Utilities;

namespace Squiggle.Core.Chat
{
    class ChatSession: IChatSession
    {
        SquiggleEndPoint localUser;
        ChatHost chatHost;
        List<IAppHandler> appSessions;
        Dictionary<string, SquiggleEndPoint> remoteUsers;
        bool initialized;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<VoiceChatInvitationReceivedEventArgs> VoiceChatInvitationReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
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

        public IEnumerable<IAppHandler> AppSessions
        {
            get { return appSessions; }
        }

        public ChatSession(Guid sessionID, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser)
        {
            this.ID = sessionID;
            this.chatHost = localHost;
            this.localUser = localUser;

            localHost.ChatInviteReceived += new EventHandler<ChatInviteReceivedEventArgs>(localHost_ChatInviteReceived);
            localHost.AppInvitationReceived += new EventHandler<AppInvitationReceivedEventArgs>(localHost_AppInvitationReceived);
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(localHost_MessageReceived);
            localHost.UserTyping += new EventHandler<SessionEventArgs>(localHost_UserTyping);
            localHost.BuzzReceived += new EventHandler<SessionEventArgs>(localHost_BuzzReceived);
            localHost.UserJoined += new EventHandler<SessionEventArgs>(localHost_UserJoined);
            localHost.UserLeft += new EventHandler<SessionEventArgs>(localHost_UserLeft);
            localHost.SessionInfoRequested += new EventHandler<SessionEventArgs>(localHost_SessionInfoRequested);
            localHost.SessionInfoReceived += new EventHandler<SessionInfoEventArgs>(localHost_SessionInfoReceived);

            remoteUsers = new Dictionary<string, SquiggleEndPoint>();
            appSessions = new List<IAppHandler>();

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

        public IFileTransfer SendFile(string name, Stream content)
        {
            if (IsGroupSession)
                throw new InvalidOperationException("Cannot send files in a group chat session.");
            long size = content.Length;
            var transfer = new FileTransfer.FileTransfer(ID, chatHost, localUser, PrimaryUser, name, size, content);
            OnAppSessionStarted(transfer);
            transfer.Start();
            return transfer;
        }

        public IVoiceChat StartVoiceChat(Dispatcher dispatcher)
        {
            if (IsGroupSession)
                throw new InvalidOperationException("Cannot start voice chat in group chat session.");
            var chat = new VoiceChat(ID, chatHost, localUser, PrimaryUser);
            chat.Dispatcher = dispatcher;
            chat.Start();
            OnAppSessionStarted(chat);
            return chat;
        }

        public void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            BroadCast(endpoint => chatHost.ReceiveMessage(ID, localUser, endpoint, fontName, fontSize, color, fontStyle, message));
        }

        public void End()
        {
            chatHost.ChatInviteReceived -= new EventHandler<ChatInviteReceivedEventArgs>(localHost_ChatInviteReceived);
            chatHost.AppInvitationReceived -= new EventHandler<AppInvitationReceivedEventArgs>(localHost_AppInvitationReceived);
            chatHost.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(localHost_MessageReceived);
            chatHost.UserTyping -= new EventHandler<SessionEventArgs>(localHost_UserTyping);
            chatHost.BuzzReceived -= new EventHandler<SessionEventArgs>(localHost_BuzzReceived);
            chatHost.UserJoined -= new EventHandler<SessionEventArgs>(localHost_UserJoined);
            chatHost.UserLeft -= new EventHandler<SessionEventArgs>(localHost_UserLeft);
            chatHost.SessionInfoRequested -= new EventHandler<SessionEventArgs>(localHost_SessionInfoRequested);
            chatHost.SessionInfoReceived -= new EventHandler<SessionInfoEventArgs>(localHost_SessionInfoReceived);

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

        void localHost_SessionInfoRequested(object sender, SessionEventArgs e)
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

        void localHost_SessionInfoReceived(object sender, SessionInfoEventArgs e)
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

        void localHost_UserLeft(object sender, SessionEventArgs e)
        {
            bool left = false;
            if (e.SessionID == ID && IsGroupSession)
                left = remoteUsers.Remove(e.Sender.ClientID);

            if (left)
                UserLeft(this, e);
        }

        void localHost_UserJoined(object sender, SessionEventArgs e)
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

        void localHost_ChatInviteReceived(object sender, ChatInviteReceivedEventArgs e)
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

        void localHost_AppInvitationReceived(object sender, AppInvitationReceivedEventArgs e)
        {
            if (e.SessionID == ID && !IsGroupSession && IsRemoteUser(e.Sender))
            {
                if (e.AppId == ChatApps.FileTransfer)
                    OnFileTransferInvite(e);
                else if (e.AppId == ChatApps.VoiceChat)
                    OnVoiceChatInvite(e);
            }
        }

        void localHost_UserTyping(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                UserTyping(this, e);
        }

        void localHost_BuzzReceived(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                BuzzReceived(this, e);
        }

        void localHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                MessageReceived(this, e);
        }

        void OnVoiceChatInvite(AppInvitationReceivedEventArgs e)
        {
            IVoiceChat invitation = new VoiceChat(ID, chatHost, localUser, PrimaryUser, e.AppSessionId);
            VoiceChatInvitationReceived(this, new VoiceChatInvitationReceivedEventArgs()
            {
                Sender = e.Sender,
                Invitation = invitation
            });
            OnAppSessionStarted((AppHandler)invitation);
        }

        void OnFileTransferInvite(AppInvitationReceivedEventArgs e)
        {
            var inviteData = new FileInviteData(e.Metadata);
            IFileTransfer invitation = new FileTransfer.FileTransfer(ID, chatHost, localUser, PrimaryUser, inviteData.Name, inviteData.Size, e.AppSessionId);
            TransferInvitationReceived(this, new FileTransferInviteEventArgs()
            {
                Sender = e.Sender,
                Invitation = invitation
            });
            OnAppSessionStarted((AppHandler)invitation);
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

        void OnAppSessionStarted(AppHandler handler)
        {
            appSessions.Add(handler);
            handler.TransferFinished += new EventHandler(handler_TransferFinished);
        }

        void handler_TransferFinished(object sender, EventArgs e)
        {
            var handler = (AppHandler)sender;
            appSessions.Remove(handler);
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
