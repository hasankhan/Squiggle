using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Squiggle.Chat.Services.Presence;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat
{
    class ChatClient: IChatClient
    {
        IChatService chatService;
        IPresenceService presenceService;
        IPEndPoint localEndPoint;

        public ChatClient(IPEndPoint localEndPoint, short presencePort, int keepAliveTime)
        {
            chatService = new ChatService();
            chatService.ResolveEndPoint += new EventHandler<ResolveEndPointEventArgs>(chatService_ResolveEndPoint);
            presenceService = new PresenceService(localEndPoint, presencePort, keepAliveTime);
            this.localEndPoint = localEndPoint;
        }

        void chatService_ResolveEndPoint(object sender, ResolveEndPointEventArgs e)
        {
            var user = presenceService.Users.FirstOrDefault(u => u.Address.ToString() == e.User);
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
