using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Chat.Services.Chat.Host;
using System.Threading;
using System.Diagnostics;

namespace Squiggle.Chat.Services.Chat
{
    public class ChatService : IChatService
    {
        ChatHost chatHost;
        ServiceHost serviceHost;
        ChatSessionCollection chatSessions;
        ChatEndPoint localEndPoint;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        public ChatService()
        {
        }        
       
        #region IChatService Members

        public void Start(ChatEndPoint endpoint)
        {
            chatHost = new ChatHost();
            chatHost.UserActivity += new EventHandler<UserActivityEventArgs>(chatHost_UserActivity);
            chatSessions = new ChatSessionCollection();

            if (serviceHost != null)
                throw new InvalidOperationException("Service already started.");

            localEndPoint = endpoint;
            serviceHost = new ServiceHost(chatHost);
            var address = CreateServiceUri(endpoint.Address.ToString());
            var binding = BindingHelper.CreateBinding();
            serviceHost.AddServiceEndpoint(typeof(IChatHost), binding, address);
            serviceHost.Open();
        }        

        public void Stop()
        {
            if (serviceHost != null)
            {                
                serviceHost.Close();
                serviceHost = null;

                chatHost.Dispose();
                chatHost = null;
                chatSessions.Clear();
            }
        }

        public IChatSession CreateSession(ChatEndPoint endPoint)
        {
            IChatSession session = chatSessions.Find(s => !s.IsGroupSession && s.RemoteUsers.Contains(endPoint));
            if (session == null)
                session = CreateSession(Guid.NewGuid(), endPoint);
            return session;
        }       

        #endregion

        void chatHost_UserActivity(object sender, UserActivityEventArgs e)
        {
            Trace.WriteLine("Ensuring chat session=" + e.SessionID);
            if (e.Type.In(ActivityType.Message, ActivityType.TransferInvite, ActivityType.Buzz, ActivityType.ChatInvite))
                EnsureChatSession(e.SessionID, e.Sender);
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/squigglechat");
            return uri;
        }

        ChatSession CreateSession(Guid sessionId, ChatEndPoint endpoint)
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

        void EnsureChatSession(Guid sessionId, ChatEndPoint user)
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
