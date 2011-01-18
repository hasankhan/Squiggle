using System;
using System.Drawing;
using System.IO;
using System.Net;
using Squiggle.Chat.Services.Chat.Host;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace Squiggle.Chat.Services.Chat
{
    class ChatSession: IChatSession
    {
        class RemoteHost
        {
            public IChatHost Host { get; set; }
            public ChatEndPoint EndPoint { get; set; }
        }

        ChatEndPoint localUser;
        ChatHost localHost;
        HashSet<ChatEndPoint> remoteUsers;
        Dictionary<string, RemoteHost> remoteHosts;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<FileTransferInviteEventArgs> TransferInvitationReceived = delegate { };
        public event EventHandler<SessionEventArgs> UserTyping = delegate { };
        public event EventHandler<SessionEventArgs> UserJoined = delegate { };
        public event EventHandler<SessionEventArgs> UserLeft = delegate { };
        public event EventHandler<SessionEventArgs> BuzzReceived = delegate { };
        public event EventHandler SessionEnded = delegate { };
        public event EventHandler GroupChatStarted = delegate { };

        public Guid ID { get; private set; }
        public IEnumerable<ChatEndPoint> RemoteUsers
        {
            get { return remoteUsers; }
        }
        public bool IsGroupSession
        {
            get { return remoteUsers.Count > 1; }
        }

        public ChatSession(Guid sessionID, ChatHost localHost, ChatEndPoint localUser, ChatEndPoint remoteUser): this(sessionID, localHost, localUser, Enumerable.Repeat(remoteUser, 1)) { }

        public ChatSession(Guid sessionID, ChatHost localHost, ChatEndPoint localUser, IEnumerable<ChatEndPoint> remoteUsers)
        {
            this.ID = sessionID;
            this.localHost = localHost;
            this.localUser = localUser;
            this.remoteUsers = new HashSet<ChatEndPoint>(remoteUsers);
            localHost.ChatInviteReceived += new EventHandler<ChatInviteReceivedEventArgs>(localHost_ChatInviteReceived);
            localHost.TransferInvitationReceived += new EventHandler<TransferInvitationReceivedEventArgs>(localHost_TransferInvitationReceived);
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
            try
            {
                SessionInfo info = PrimaryHost.Host.GetSessionInfo(ID, localUser, PrimaryHost.EndPoint);
                if (info != null && info.Participants != null)
                {
                    bool wasGroupSession = IsGroupSession;
                    AddParticipants(info.Participants);
                    if (!wasGroupSession && IsGroupSession)
                        GroupChatStarted(this, EventArgs.Empty);
                }
            }
            catch (Exception ex) 
            {
                Trace.WriteLine("Could not get session info due to exception: " + ex.Message);
            }
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
                try
                {
                    AddParticipants(e.Participants);
                    BroadCast((host, endpoint) => host.JoinChat(ID, localUser, endpoint));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Could not respond to chat invite due to exception: " + ex.Message);
                }
                GroupChatStarted(this, EventArgs.Empty);
            }
        }       

        void localHost_TransferInvitationReceived(object sender, TransferInvitationReceivedEventArgs e)
        {
            if (e.SessionID == ID && !IsGroupSession)
            {
                if (IsRemoteUser(e.Sender))
                {
                    RemoteHost remoteHost = PrimaryHost;
                    IFileTransfer invitation = new FileTransfer(ID, remoteHost.Host, localHost, localUser, remoteHost.EndPoint, e.Name, e.Size, e.ID);
                    TransferInvitationReceived(this, new FileTransferInviteEventArgs()
                    {
                        Sender = localUser,
                        Invitation = invitation
                    });
                }
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
            var transfer = new FileTransfer(ID, remoteHost.Host, localHost, localUser, remoteHost.EndPoint, name, size, content);
            transfer.Start();
            return transfer;
        }

        public void SendMessage(string fontName, int fontSize, Color color, FontStyle fontStyle, string message)
        {
            BroadCast((host, endpoint) => host.ReceiveMessage(ID, localUser, endpoint, fontName, fontSize, color, fontStyle, message));
        }

        public void End()
        {
            localHost.ChatInviteReceived -= new EventHandler<ChatInviteReceivedEventArgs>(localHost_ChatInviteReceived);
            localHost.TransferInvitationReceived -= new EventHandler<TransferInvitationReceivedEventArgs>(localHost_TransferInvitationReceived);
            localHost.MessageReceived -= new EventHandler<MessageReceivedEventArgs>(host_MessageReceived);
            localHost.UserTyping -= new EventHandler<SessionEventArgs>(localHost_UserTyping);
            localHost.BuzzReceived -= new EventHandler<SessionEventArgs>(localHost_BuzzReceived);
            localHost.UserJoined -= new EventHandler<SessionEventArgs>(localHost_UserJoined);
            localHost.UserLeft -= new EventHandler<SessionEventArgs>(localHost_UserLeft);
            localHost.SessionInfoRequested -= new EventHandler<SessionInfoRequestedEventArgs>(localHost_SessionInfoRequested);
            try
            {
                BroadCast((host, endpoint) => host.LeaveChat(ID, localUser, endpoint));
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Could not send leave message due to exception: " + ex.Message);
            }
            SessionEnded(this, EventArgs.Empty);
        }

        public void Invite(ChatEndPoint user)
        {
            var proxy = ChatHostProxyFactory.Get(user.Address);
            proxy.ReceiveChatInvite(ID, localUser, user, remoteUsers.ToArray());
        }

        bool IsRemoteUser(ChatEndPoint user)
        {
            return remoteUsers.Contains(user);
        }

        void CreateRemoteHosts()
        {
            lock (remoteHosts)
                foreach (ChatEndPoint user in RemoteUsers)
                    remoteHosts[user.ClientID] = new RemoteHost()
                    {
                        Host = ChatHostProxyFactory.Get(user.Address),
                        EndPoint = user
                    };
        }

        void BroadCast(Action<IChatHost, ChatEndPoint> hostAction)
        {
            BroadCast(hostAction, true);
        }

        void BroadCast(Action<IChatHost, ChatEndPoint> hostAction, bool continueOnError)
        {
            bool allSuccess = true;

            IEnumerable<RemoteHost> hosts;
            lock (remoteHosts)
                hosts = remoteHosts.Values.ToList(); 
            foreach (RemoteHost host in hosts)
                try
                {
                    hostAction(host.Host, host.EndPoint);
                }
                catch (Exception ex)
                {
                    allSuccess = false;
                    if (continueOnError)
                        Trace.WriteLine(ex.Message);
                    else
                        throw;
                }

            if (continueOnError && !allSuccess)
                throw new OperationFailedException();
        }

        void AddParticipants(ChatEndPoint[] participants)
        {
            foreach (ChatEndPoint user in participants)
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
