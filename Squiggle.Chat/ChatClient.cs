using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Squiggle.Chat
{
    /* This class will be used by the WPF front end for all communication */
    class ChatClient: IChatClient
    {
        IChatService chatService;
        IPresenceService presenceService;
        IPEndPoint localEndPoint;

        public ChatClient(IPEndPoint localEndPoint)
        {
            chatService = new ChatService();
            chatService.ResolveEndPoint += new EventHandler<ResolveEndPointEventArgs>(chatService_ResolveEndPoint);
            presenceService = new PresenceService();
            this.localEndPoint = localEndPoint;
        }

        void chatService_ResolveEndPoint(object sender, ResolveEndPointEventArgs e)
        {
            var user = presenceService.Users.FirstOrDefault(u => u.Name == e.Username);
            if (user != null)
                e.EndPoint = user.ChatEndPoint;
        }

        public void Login(string username)
        {
            presenceService.Login(username);
            chatService.Start(localEndPoint);
        }

        public void Logout()
        {
            chatService.Stop();
            presenceService.Logout();
        }
    }
}
