using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Chat.Services.Chat.Host;
using System.Threading;

namespace Squiggle.Chat.Services.Chat
{
    public class ChatService : IChatService
    {
        ChatHost chatHost;
        ServiceHost serviceHost;
        ChatSessionCollection chatSessions;
        IPEndPoint localEndPoint;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        public string Username { get; set; }
        
        public ChatService()
        {
        }        
       
        #region IChatService Members

        public void Start(IPEndPoint endpoint)
        {
            chatHost = new ChatHost();
            chatHost.UserActivity += new EventHandler<UserActivityEventArgs>(chatHost_UserActivity);
            chatSessions = new ChatSessionCollection();

            if (serviceHost != null)
                throw new InvalidOperationException("Service already started.");

            localEndPoint = endpoint;
            serviceHost = new ServiceHost(chatHost);
            var address = CreateServiceUri(endpoint.ToString());
            var binding = new NetTcpBinding(SecurityMode.None);
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

        public IChatSession CreateSession(IPEndPoint endPoint)
        {
            IChatSession session = chatSessions.Find(s=>!s.IsGroupSession && s.RemoteUsers.Contains(endPoint));
            if (session == null)
                session = CreateSession(Guid.NewGuid(), endPoint);
            return session;
        }       

        #endregion

        void chatHost_UserActivity(object sender, UserActivityEventArgs e)
        {
            if (e.Type == ActivityType.Message || 
                e.Type == ActivityType.TransferInvite || 
                e.Type == ActivityType.Buzz || 
                e.Type == ActivityType.ChatInvite)
                EnsureChatSession(e.SessionID, e.User);
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/squigglechat");
            return uri;
        }

        ChatSession CreateSession(Guid sessionId, IPEndPoint endPoint)
        {
            ChatSession session = new ChatSession(sessionId, chatHost, localEndPoint, endPoint);
            RegisterSession(session);
            return session;
        }

        void RegisterSession(ChatSession session)
        {
            session.SessionEnded += (sender, e) => chatSessions.Remove(session);
            this.chatSessions.Add(session);
        } 

        void EnsureChatSession(Guid sessionId, IPEndPoint user)
        {
            if (!chatSessions.Contains(sessionId))
            {
                var session = CreateSession(sessionId, user);
                ChatStarted(this, new ChatStartedEventArgs() { Session = session });
                ThreadPool.QueueUserWorkItem(_=>session.UpdateSessionInfo());
            }
        }
    }
}
