using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;
using Squiggle.Core.Chat.Transport.Messages;

namespace Squiggle.Core.Chat
{
    public class ChatService : IChatService
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

        public IChatSession CreateSession(ISquiggleEndPoint endPoint)
        {
            IChatSession result = chatSessions.Find(s => !s.IsGroupSession && s.RemoteUsers.Contains(endPoint));
            if (result == null)
            {
                var session = CreateSession(Guid.NewGuid(), endPoint);
                session.Initialize(false);
                result = session;
            }
            return result;
        }       

        #endregion

        public void Start()
        {
            chatHost = new ChatHost(localEndPoint.Address);
            chatHost.Start();
            chatHost.MessageReceived += chatHost_MessageReceived;
            chatSessions = new ChatSessionCollection();
        }

        public void Stop()
        {
            if (chatHost != null)
            {
                chatHost.MessageReceived -= chatHost_MessageReceived;
                chatHost.Dispose();
                chatHost = null;
                chatSessions.Clear();
            }
        }

        void chatHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Trace.WriteLine("Ensuring chat session=" + e.SessionID);
            if (e.Type.In(typeof(TextMessage), typeof(ActivityInviteMessage), typeof(BuzzMessage), typeof(ChatInviteMessage)))
                EnsureChatSession(e.SessionID, e.Sender);
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/" + ServiceNames.ChatService);
            return uri;
        }

        ChatSession CreateSession(Guid sessionId, ISquiggleEndPoint endpoint)
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

        void EnsureChatSession(Guid sessionId, ISquiggleEndPoint user)
        {
            if (!chatSessions.Contains(sessionId))
            {
                var session = CreateSession(sessionId, user);
                ChatStarted(this, new ChatStartedEventArgs(){ Session = session});
                session.Initialize(true);
            }
        }
    }
}
