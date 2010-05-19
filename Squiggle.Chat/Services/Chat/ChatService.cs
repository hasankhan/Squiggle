using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ServiceModel;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Chat.Services.Chat
{
    public class ChatService : IChatService
    {
        ChatHost chatHost;
        ServiceHost serviceHost;
        Dictionary<IPEndPoint, IChatSession> chatSessions;
        IPEndPoint localEndPoint;

        public string Username { get; set; }
        
        public ChatService()
        {
            chatHost = new ChatHost();
            chatHost.MessageReceived += new EventHandler<MessageReceivedEventArgs>(chatHost_MessageReceived);
            chatSessions = new Dictionary<IPEndPoint, IChatSession>();
        }        

        #region IChatService Members

        public void Start(IPEndPoint endpoint)
        {
            if (serviceHost != null)
                throw new InvalidOperationException("Service already started.");

            localEndPoint = endpoint;
            serviceHost = new ServiceHost(chatHost);
            var address = CreateServiceUri(endpoint.ToString());
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

        public IChatSession CreateSession(IPEndPoint endPoint)
        {
            IChatSession session;
            if (!chatSessions.TryGetValue(endPoint, out session))
            {
                Uri uri = CreateServiceUri(endPoint.ToString());
                var binding = new NetTcpBinding();
                IChatHost remoteHost = new ChatHostProxy(binding, new EndpointAddress(uri));
                ChatSession temp = new ChatSession(chatHost, remoteHost, localEndPoint, endPoint);
                temp.SessionEnded += (sender, e) => chatSessions.Remove(temp.RemoteUser);
                this.chatSessions.Add(endPoint, session);
                session = temp;
            }
            return session;
        }

        public event EventHandler<ChatStartedEventArgs> ChatStarted = delegate { };

        #endregion

        void chatHost_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (!chatSessions.ContainsKey(e.User))
            {
                var session = CreateSession(e.User);
                ChatStarted(this, new ChatStartedEventArgs() { Message=e.Message, Session = session });
            }
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/squiggle");
            return uri;
        }
    }
}
