using System;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;
using Squiggle.Chat.Services.Chat;

namespace Squiggle.Bridge
{
    public class MessageReceivedEventArgs: EventArgs
    {
        public Message Message {get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class BridgeHost: IBridgeHost
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        public void ReceiveMessage(byte[] message)
        {
            var msg = Message.Deserialize(message);
            MessageReceived(this, new MessageReceivedEventArgs() { Message = msg });
        }

        public Chat.Services.Presence.UserInfo GetUserInfo()
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(ChatEndPoint sender, byte[] message)
        {
            throw new NotImplementedException();
        }

        public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void Buzz(Guid sessionId, ChatEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void UserIsTyping(Guid sessionId, ChatEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(Guid sessionId, ChatEndPoint user, string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
        {
            throw new NotImplementedException();
        }

        public void ReceiveChatInvite(Guid sessionId, ChatEndPoint user, ChatEndPoint[] participants)
        {
            throw new NotImplementedException();
        }

        public void JoinChat(Guid sessionId, ChatEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void LeaveChat(Guid sessionId, ChatEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFileInvite(Guid sessionId, ChatEndPoint user, Guid id, string name, long size)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFileContent(Guid id, byte[] chunk)
        {
            throw new NotImplementedException();
        }

        public void AcceptFileInvite(Guid id)
        {
            throw new NotImplementedException();
        }

        public void CancelFileTransfer(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
