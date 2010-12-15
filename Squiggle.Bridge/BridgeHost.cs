using System;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;

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

        public void ReceiveMessage(System.Net.IPEndPoint sender, byte[] message)
        {
            throw new NotImplementedException();
        }

        public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, System.Net.IPEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void Buzz(Guid sessionId, System.Net.IPEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void UserIsTyping(Guid sessionId, System.Net.IPEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(Guid sessionId, System.Net.IPEndPoint user, string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
        {
            throw new NotImplementedException();
        }

        public void ReceiveChatInvite(Guid sessionId, System.Net.IPEndPoint user, System.Net.IPEndPoint[] participants)
        {
            throw new NotImplementedException();
        }

        public void JoinChat(Guid sessionId, System.Net.IPEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void LeaveChat(Guid sessionId, System.Net.IPEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFileInvite(Guid sessionId, System.Net.IPEndPoint user, Guid id, string name, long size)
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
