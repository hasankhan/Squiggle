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

        public ChatClient(IPEndPoint localEndPoint)
        {
            chatService = new ChatService();
            chatService.ResolveEndPoint += new EventHandler<ResolveEndPointEventArgs>(chatService_ResolveEndPoint);
            presenceService = new PresenceService();
            this.localEndPoint = localEndPoint;
        }

        void chatService_ResolveEndPoint(object sender, ResolveEndPointEventArgs e)
        {
            var user = presenceService.Users.FirstOrDefault(u => u.UserFriendlyName == e.Username);
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
