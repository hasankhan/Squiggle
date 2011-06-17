using System;
using System.Drawing;
using System.IO;
using System.Net;
using Squiggle.Chat.Services.Chat.Host;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Squiggle.Utilities;
using Squiggle.Chat.Services.Chat.FileTransfer;
using Squiggle.Chat.Services.Chat.Audio;
using System.Windows.Threading;

namespace Squiggle.Chat.Services.Chat
{
    class ChatSession: IChatSession
    {
        class RemoteHost
        {
            public IChatHost Host { get; set; }
            public SquiggleEndPoint EndPoint { get; set; }
        }

        SquiggleEndPoint localUser;
        ChatHost localHost;
        HashSet<SquiggleEndPoint> remoteUsers;
        Dictionary<string, RemoteHost> remoteHosts;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<VoiceChatInvitationReceivedEventArgs> VoiceChatInvitationReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler SessionEnded = delegate { };
        public event EventHandler GroupChatStarted = delegate { };

        public Guid ID { get; private set; }
        public IEnumerable<SquiggleEndPoint> RemoteUsers
        {
            get { return remoteUsers; }
        }
        public bool IsGroupSession
        {
            get { return remoteUsers.Count > 1; }
        }

        public ChatSession(Guid sessionID, ChatHost localHost, SquiggleEndPoint localUser, SquiggleEndPoint remoteUser): this(sessionID, localHost, localUser, Enumerable.Repeat(remoteUser, 1)) { }

        public ChatSession(Guid sessionID, ChatHost localHost, SquiggleEndPoint localUser, IEnumerable<SquiggleEndPoint> remoteUsers)
        {
            this.ID = sessionID;
            this.localHost = localHost;
            this.localUser = localUser;
            this.remoteUsers = new HashSet<SquiggleEndPoint>(remoteUsers);
            localHost.ChatInviteReceived += new EventHandler<ChatInviteReceivedEventArgs>(localHost_ChatInviteReceived);
            localHost.AppInvitationReceived += new EventHandler<AppInvitationReceivedEventArgs>(localHost_AppInvitationReceived);
            localHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            localHost.UserTyping += new EventHandler<SessionEventArgs>(localHost_UserTyping);
            localHost.BuzzReceived += new EventHandler<SessionEventArgs>(localHost_BuzzReceived);
            localHost.UserJoined += new EventHandler<SessionEventArgs>(localHost_UserJoined);
            localHost.UserLeft += new EventHandler<SessionEventArgs>(localHost_UserLeft);
            localHost.SessionInfoRequested += new EventHandler<SessionInfoRequestedEventArgs>(localHost_SessionInfoRequested);
            remoteHosts = new Dictionary<string, RemoteHost>();
            CreateRemoteHosts();
        }

        RemoteHost PrimaryHost
        {
            get
            {
                RemoteHost remoteHost;
                lock (remoteHosts)
                    remoteHost = remoteHosts.FirstOrDefault().Value;
                return remoteHost;
            }
        }

        public void UpdateSessionInfo()
        {
            ExceptionMonster.EatTheException(()=>
            {
                SessionInfo info = PrimaryHost.Host.GetSessionInfo(ID, localUser, PrimaryHost.EndPoint);
                if (info != null && info.Participants != null)
                {
                    bool wasGroupSession = IsGroupSession;
                    AddParticipants(info.Participants);
                    if (!wasGroupSession && IsGroupSession)
                        GroupChatStarted(this, EventArgs.Empty);
                }
            },"getting session info");
        }

        void localHost_SessionInfoRequested(object sender, SessionInfoRequestedEventArgs e)
        {
            if (e.SessionID == ID)
                e.Info.Participants = remoteUsers.Except(Enumerable.Repeat(e.Sender, 1)).ToArray();
        }

        void localHost_UserLeft(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID && IsGroupSession)
                if (remoteUsers.Remove(e.Sender))
                {
                    remoteHosts.Remove(e.Sender.ClientID);
                    UserLeft(this, e);
                }
        }

        void localHost_UserJoined(object sender, SessionEventArgs e)
        {
            if (e.SessionID == ID && remoteUsers.Add(e.Sender))
            {
                var proxy = ChatHostProxyFactory.Get(e.Sender.Address);
                remoteHosts[e.Sender.ClientID] = new RemoteHost() 
                { 
                    Host = proxy, 
                    EndPoint = e.Sender
                };
                UserJoined(this, e);
            }
        }        

        void localHost_ChatInviteReceived(object sender, ChatInviteReceivedEventArgs e)
        {
            if (e.SessionID == ID)
            {
                ExceptionMonster.EatTheException(() =>
                {
                    AddParticipants(e.Participants);
                    BroadCast((host, endpoint) => host.JoinChat(ID, localUser, endpoint));
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

        void OnVoiceChatInvite(AppInvitationReceivedEventArgs e)
        {
            RemoteHost remoteHost = PrimaryHost;
            IVoiceChat invitation = new VoiceChat(ID, remoteHost.Host, localHost, localUser, remoteHost.EndPoint, e.AppSessionId);
            VoiceChatInvitationReceived(this, new VoiceChatInvitationReceivedEventArgs()
            {
                Sender = e.Sender,
                Invitation = invitation
            });
        }

        void OnFileTransferInvite(AppInvitationReceivedEventArgs e)
        {
            var inviteData = new FileInviteData(e.Metadata);
            RemoteHost remoteHost = PrimaryHost;
            IFileTransfer invitation = new FileTransfer.FileTransfer(ID, remoteHost.Host, localHost, localUser, remoteHost.EndPoint, inviteData.Name, inviteData.Size, e.AppSessionId);
            TransferInvitationReceived(this, new FileTransferInviteEventArgs()
            {
                Sender = e.Sender,
                Invitation = invitation
            });
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

        void host_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.SessionID == ID && IsRemoteUser(e.Sender))
                MessageReceived(this, e);
        }

        public void SendBuzz()
        {
            BroadCast((host, endpoint) => host.Buzz(ID, localUser, endpoint));
        }

        public void NotifyTyping()
        {
            BroadCast((host, endpoint) => host.UserIsTyping(ID, localUser, endpoint));
        }

        public IFileTransfer SendFile(string name, Stream content)
        {
            if (IsGroupSession)
                throw new InvalidOperationException("Cannot send files in a group chat session.");
            RemoteHost remoteHost = PrimaryHost;
            long size = content.Length;
            var transfer = new FileTransfer.FileTransfer(ID, remoteHost.Host, localHost, localUser, remoteHost.EndPoint, name, size, content);
            transfer.Start();
            return transfer;
        }

        public IVoiceChat StartVoiceChat(Dispatcher dispatcher)
        {
            if (IsGroupSession)
                throw new InvalidOperationException("Cannot start voice chat in group chat session.");
            RemoteHost remoteHost = PrimaryHost;
            var chat = new VoiceChat(ID, remoteHost.Host, localHost, localUser, remoteHost.EndPoint);
            chat.Dispatcher = dispatcher;
            chat.Start();
            return chat;
        }

        public void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            BroadCast((host, endpoint) => host.ReceiveMessage(ID, localUser, endpoint, fontName, fontSize, color, fontStyle, message));
        }

        public void End()
        {
            localHost.ChatInviteReceived -= new EventHandler<ChatInviteReceivedEventArgs>(localHost_ChatInviteReceived);
            localHost.AppInvitationReceived -= new EventHandler<AppInvitationReceivedEventArgs>(localHost_AppInvitationReceived);
            localHost.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            localHost.UserTyping -= new EventHandler<SessionEventArgs>(localHost_UserTyping);
            localHost.BuzzReceived -= new EventHandler<SessionEventArgs>(localHost_BuzzReceived);
            localHost.UserJoined -= new EventHandler<SessionEventArgs>(localHost_UserJoined);
            localHost.UserLeft -= new EventHandler<SessionEventArgs>(localHost_UserLeft);
            localHost.SessionInfoRequested -= new EventHandler<SessionInfoRequestedEventArgs>(localHost_SessionInfoRequested);

            ExceptionMonster.EatTheException(() =>
            {
                BroadCast((host, endpoint) => host.LeaveChat(ID, localUser, endpoint));
            }, "sending leave message");
            SessionEnded(this, EventArgs.Empty);
        }

        public void Invite(SquiggleEndPoint user)
        {
            var proxy = ChatHostProxyFactory.Get(user.Address);
            proxy.ReceiveChatInvite(ID, localUser, user, remoteUsers.ToArray());
        }

        bool IsRemoteUser(SquiggleEndPoint user)
        {
            return remoteUsers.Contains(user);
        }

        void CreateRemoteHosts()
        {
            lock (remoteHosts)
                foreach (SquiggleEndPoint user in RemoteUsers)
                    remoteHosts[user.ClientID] = new RemoteHost()
                    {
                        Host = ChatHostProxyFactory.Get(user.Address),
                        EndPoint = user
                    };
        }

        void BroadCast(Action<IChatHost, SquiggleEndPoint> hostAction)
        {
            BroadCast(hostAction, true);
        }

        void BroadCast(Action<IChatHost, SquiggleEndPoint> hostAction, bool continueOnError)
        {
            bool allSuccess = true;

            IEnumerable<RemoteHost> hosts;
            lock (remoteHosts)
                hosts = remoteHosts.Values.ToList();
            foreach (RemoteHost host in hosts)
            {
                Exception ex;
                if (!ExceptionMonster.EatTheException(()=>
                    {
                        hostAction(host.Host, host.EndPoint);
                    },"doing a broadcast operation in chat", out ex))
                {
                    allSuccess = false;
                    if (continueOnError)
                        Trace.WriteLine(ex.Message);
                    else
                        throw ex;
                }
            }

            if (continueOnError && !allSuccess)
                throw new OperationFailedException();
        }

        void AddParticipants(SquiggleEndPoint[] participants)
        {
            foreach (SquiggleEndPoint user in participants)
                remoteUsers.Add(user);

            CreateRemoteHosts();
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
