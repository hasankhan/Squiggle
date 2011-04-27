using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Chat.Services.Chat.Host;
using System.Threading;
using System.Diagnostics;
using Squiggle.Utilities;

namespace Squiggle.Chat.Services.Chat
{
    public class ChatService : WcfHost, IChatService
    {
        ChatHost chatHost;
        ChatSessionCollection chatSessions;
        SquiggleEndPoint localEndPoint;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        public ChatService(SquiggleEndPoint endpoint)
        {
            localEndPoint = endpoint;
        }                      

        #region IChatService Members

        public IChatSession CreateSession(SquiggleEndPoint endPoint)
        {
            IChatSession session = chatSessions.Find(s => !s.IsGroupSession && s.RemoteUsers.Contains(endPoint));
            if (session == null)
                session = CreateSession(Guid.NewGuid(), endPoint);
            return session;
        }       

        #endregion

        protected override void OnStart()
        {
            chatHost = new ChatHost();
            chatHost.UserActivity += new EventHandler<UserActivityEventArgs>(chatHost_UserActivity);
            chatSessions = new ChatSessionCollection();

            base.OnStart();
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (chatHost != null)
            {
                chatHost.UserActivity -= new EventHandler<UserActivityEventArgs>(chatHost_UserActivity);
                chatHost = null;
                chatSessions.Clear();
            }
        }

        protected override ServiceHost CreateHost()
        {
            var serviceHost = new ServiceHost(chatHost);

            var address = CreateServiceUri(localEndPoint.Address.ToString());
            var binding = WcfConfig.CreateBinding();
            serviceHost.AddServiceEndpoint(typeof(IChatHost), binding, address);

            return serviceHost;
        }

        void chatHost_UserActivity(object sender, UserActivityEventArgs e)
        {
            Trace.WriteLine("Ensuring chat session=" + e.SessionID);
            if (e.Type.In(ActivityType.Message, ActivityType.TransferInvite, ActivityType.Buzz, ActivityType.ChatInvite))
                EnsureChatSession(e.SessionID, e.Sender);
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/" + ServiceNames.ChatService);
            return uri;
        }

        ChatSession CreateSession(Guid sessionId, SquiggleEndPoint endpoint)
        {
            ChatSession session = new ChatSession(sessionId, chatHost, localEndPoint, endpoint);
            RegisterSession(session);
            return session;
        }

        void RegisterSession(ChatSession session)
        {
            session.SessionEnded += (sender, e) => chatSessions.Remove(session);
            this.chatSessions.Add(session);
        } 

        void EnsureChatSession(Guid sessionId, SquiggleEndPoint user)
        {
            if (!chatSessions.Contains(sessionId))
            {
                var session = CreateSession(sessionId, user);
                session.UpdateSessionInfo();
                ChatStarted(this, new ChatStartedEventArgs() { Session = session });
            }
        }
    }
}
