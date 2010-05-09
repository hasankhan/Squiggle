using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Squiggle.Chat.Service;
using System.ServiceModel;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Chat.Services.Chat
{
    class ResolveEndPointEventArgs:EventArgs
    {
        public string User {get; set;}
        public IPEndPoint EndPoint {get; set;}
    }

    class ChatService: IChatService
    {
        ChatHost chatHost;
        ServiceHost serviceHost;
        Dictionary<string, IChatSession> chatSessions;

        public string Username { get; set; }
        public event EventHandler<ResolveEndPointEventArgs> ResolveEndPoint = delegate { };
        
        public ChatService()
        {
            chatHost = new ChatHost();
            chatHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(chatHost_MessageReceived);
            chatSessions = new Dictionary<string, IChatSession>();
        }        

        #region IChatService Members

        public void Start(IPEndPoint endpoint)
        {
            if (serviceHost != null)
                throw new InvalidOperationException("Service already started.");

            serviceHost = new ServiceHost(chatHost);
            var address = CreateServiceUri(endpoint);
            serviceHost.AddServiceEndpoint(typeof(IChatHost), new NetTcpBinding(), address);
            serviceHost.Open();
        }        

        public void Stop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }

        public IChatSession CreateSession(IPEndPoint host)
        {
            IChatSession session;
            if (!chatSessions.TryGetValue(host.Address.ToString(), out session))
            {
                Uri addess = CreateServiceUri(host);
                IChatHost remoteHost = new ChatHostProxy(new NetTcpBinding(), new EndpointAddress(addess.ToString()));
                session = new ChatSession(chatHost, remoteHost, Username);
                this.chatSessions.Add(host.Address.ToString(), session);
            }
            return session;
        }

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        #endregion

        void chatHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!chatSessions.ContainsKey(e.User))
            {
                var args = new ResolveEndPointEventArgs(){User = e.User };
                ResolveEndPoint(this, args);
                if (args.EndPoint != null)
                {
                    var session = CreateSession(args.EndPoint);
                    ChatStarted(this, new ChatStartedEventArgs() { Session = session });
                }
            }
        }

        static Uri CreateServiceUri(IPEndPoint endpoint)
        {
            var address = new Uri("net.tcp://" + endpoint.ToString() + "/chat");
            return address;
        }
    }
}
