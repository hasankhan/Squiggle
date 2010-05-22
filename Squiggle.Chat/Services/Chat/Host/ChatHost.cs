using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using System.Diagnostics;

namespace Squiggle.Chat.Services.Chat.Host
{
    public  class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint User { get; set; }
        public string Message { get; set; }
    }

    public class UserEventArgs: EventArgs
    {
        public IPEndPoint User {get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class ChatHost: IChatHost
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };
        public event EventHandler<UserEventArgs> UserTyping = delegate { };

        #region IChatHost Members

        public void UserIsTyping(IPEndPoint user)
        {
            UserTyping(this, new UserEventArgs() { User = user });
            Trace.WriteLine(user.ToString() + " is typing.");
        }

        public void ReceiveMessage(IPEndPoint user, string message)
        {            
            MessageReceived(this, new MessageReceivedEventArgs() { User = user, Message = message });
            Trace.WriteLine("Message received from: " + user.ToString() + ", message = " + message);
        }

        #endregion
    }
}
