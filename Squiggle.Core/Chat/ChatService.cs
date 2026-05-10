using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Utilities;
using Squiggle.Core.Chat.Transport.Messages;

namespace Squiggle.Core.Chat
{
    public class ChatService : IChatService
    {
        ChatHost chatHost = null!;
        ChatSessionCollection chatSessions = null!;
        SquiggleEndPoint localEndPoint;
        readonly ILoggerFactory loggerFactory;
        readonly ILogger<ChatService> logger;

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        public ChatService(SquiggleEndPoint endpoint, ILoggerFactory? loggerFactory = null)
        {
            localEndPoint = endpoint;
            this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            this.logger = this.loggerFactory.CreateLogger<ChatService>();
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
            chatHost = new ChatHost(localEndPoint.Address, loggerFactory.CreateLogger<ChatHost>());
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

        void chatHost_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            logger.LogDebug("Ensuring chat session={SessionId}", e.SessionID);
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
            ChatSession session = new ChatSession(sessionId, chatHost, localEndPoint, endpoint, loggerFactory.CreateLogger<ChatSession>());
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
