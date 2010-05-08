using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Squiggle.Chat.Service;
using System.ServiceModel;

namespace Squiggle.Chat
{
    class ChatService: IChatService
    {
        ChatHost chatHost;
        ServiceHost serviceHost;
        Dictionary<string, IChatSession> chatSessions;

        public string Username { get; set; }

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

        public IChatSession CreateSession(IPEndPoint host, string remoteUser)
        {
            IChatSession session;
            if (!chatSessions.TryGetValue(remoteUser, out session))
            {
                Uri addess = CreateServiceUri(host);
                IChatHost remoteHost = new ChatHostProxy(new NetTcpBinding(), new EndpointAddress(addess.ToString()));
                session = new ChatSession(chatHost, remoteHost, Username, remoteUser);
                this.chatSessions.Add(remoteUser, session);
            }
            return session;
        }

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        #endregion

        void chatHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!chatSessions.ContainsKey(e.User))
            {
                //TODO: get the address remote user address from presence service
                var session = CreateSession(new IPEndPoint(IPAddress.Loopback, 34), e.User);
                ChatStarted(this, new ChatStartedEventArgs() { Session = session });
            }
        }

        static Uri CreateServiceUri(IPEndPoint endpoint)
        {
            var address = new Uri("net.tcp://" + endpoint.ToString() + "/chat");
            return address;
        }
    }
}
