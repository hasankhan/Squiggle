using System;
using System.Net;
using System.ServiceModel;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Chat.Services.Presence.Transport.Host
{
    public class UserInfoRequestedEventArgs: EventArgs
    {
        public ChatEndPoint User { get; private set; }
        public UserInfo UserInfo { get; set; }

        public UserInfoRequestedEventArgs(ChatEndPoint user)
        {
            this.User = user;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    class PresenceHost: IPresenceHost
    {
        public event EventHandler<UserInfoRequestedEventArgs> UserInfoRequested = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceivePresenceMessage(ChatEndPoint sender, ChatEndPoint recepient, byte[] message)
        {
            var msg = Message.Deserialize(message);
            MessageReceived(this, new MessageReceivedEventArgs() 
            { 
                Message = msg, 
                Sender = sender 
            });
        }

        public UserInfo GetUserInfo(ChatEndPoint user)
        {
            var args = new UserInfoRequestedEventArgs(user);
            UserInfoRequested(this, args);
            return args.UserInfo;
        }
    }
}
