using System;
using System.Net;
using System.ServiceModel;
using Squiggle.Core.Chat;

namespace Squiggle.Core.Presence.Transport.Host
{
    public class UserInfoRequestedEventArgs: EventArgs
    {
        public SquiggleEndPoint User { get; private set; }
        public UserInfo UserInfo { get; set; }

        public UserInfoRequestedEventArgs(SquiggleEndPoint user)
        {
            this.User = user;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] 
    class PresenceHost: IPresenceHost
    {
        public event EventHandler<UserInfoRequestedEventArgs> UserInfoRequested = delegate { };
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceivePresenceMessage(SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] message)
        {
            var msg = Message.Deserialize(message);
            MessageReceived(this, new MessageReceivedEventArgs() 
            { 
                Recipient = recepient,
                Message = msg, 
                Sender = sender 
            });
        }

        public UserInfo GetUserInfo(SquiggleEndPoint user)
        {
            var args = new UserInfoRequestedEventArgs(user);
            UserInfoRequested(this, args);
            return args.UserInfo;
        }
    }
}
