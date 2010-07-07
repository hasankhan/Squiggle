using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Chat.Services.Chat.Host;

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
            IChatSession session = chatSessions.FindSessions(endPoint).FirstOrDefault(s=>!s.IsGroupSession);
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

        IChatSession CreateSession(Guid sessionId, IPEndPoint endPoint)
        {
            ChatSession temp = new ChatSession(sessionId, chatHost, localEndPoint, endPoint);
            temp.SessionEnded += (sender, e) => chatSessions.Remove(temp);
            this.chatSessions.Add(temp);
            return temp;
        } 

        void EnsureChatSession(Guid sessionId, IPEndPoint user)
        {
            if (!chatSessions.Contains(sessionId))
            {
                var session = CreateSession(sessionId, user);
                ChatStarted(this, new ChatStartedEventArgs() { Session = session });
            }
        }
    }
}
