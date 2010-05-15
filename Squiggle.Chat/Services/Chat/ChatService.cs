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
    public class ResolveEndPointEventArgs:EventArgs
    {
        public string User {get; set;}
        public IPEndPoint EndPoint {get; set;}
    }

    public class ChatService : IChatService
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

        public IChatSession CreateSession(IPEndPoint endpoint)
        {
            IChatSession session;
            var address = endpoint.ToString();
            if (!chatSessions.TryGetValue(address, out session))
            {
                Uri uri = CreateServiceUri(address);
                var binding =new NetTcpBinding();
                //temp code to resolve the "server rejected credentials" exception
                binding.Security.Mode = SecurityMode.Transport;
                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

                IChatHost remoteHost = new ChatHostProxy(binding, new EndpointAddress(uri));
                session = new ChatSession(chatHost, remoteHost, Username);
                this.chatSessions.Add(address, session);
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
                    ChatStarted(this, new ChatStartedEventArgs() { Address = e.User, Session = session });
                }
            }
        }

        static Uri CreateServiceUri(string address)
        {
            var uri = new Uri("net.tcp://" + address + "/chat");
            return uri;
        }
    }
}
