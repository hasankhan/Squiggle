using System;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;
using Squiggle.Chat.Services.Chat;
using System.Net;

namespace Squiggle.Bridge
{
    public class PresenceMessageForwardedEventArgs: EventArgs
    {
        public IPEndPoint BridgeEndPoint { get; set; }
        public Message Message {get; set; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)] 
    public class BridgeHost: IBridgeHost
    {
        public event EventHandler<PresenceMessageForwardedEventArgs> PresenceMessageForwarded = delegate { };

        public void ForwardPresenceMessage(byte[] message, IPEndPoint bridgeEndPoint)
        {
            var msg = Message.Deserialize(message);
            var args = new PresenceMessageForwardedEventArgs() 
                        { 
                            Message = msg, 
                            BridgeEndPoint = bridgeEndPoint 
                        };
            PresenceMessageForwarded(this, args);
        }

        public Chat.Services.Presence.UserInfo GetUserInfo(ChatEndPoint user)
        {
            throw new NotImplementedException();
        }

        public void ReceivePresenceMessage(ChatEndPoint sender, ChatEndPoint recepient, byte[] message)
        {
            throw new NotImplementedException();
        }

        public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }

        public void Buzz(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }

        public void UserIsTyping(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }

        public void ReceiveMessage(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
        {
            throw new NotImplementedException();
        }

        public void ReceiveChatInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, ChatEndPoint[] participants)
        {
            throw new NotImplementedException();
        }

        public void JoinChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }

        public void LeaveChat(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFileInvite(Guid sessionId, ChatEndPoint sender, ChatEndPoint recepient, Guid id, string name, long size)
        {
            throw new NotImplementedException();
        }

        public void ReceiveFileContent(Guid id, ChatEndPoint sender, ChatEndPoint recepient, byte[] chunk)
        {
            throw new NotImplementedException();
        }

        public void AcceptFileInvite(Guid id, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }

        public void CancelFileTransfer(Guid id, ChatEndPoint sender, ChatEndPoint recepient)
        {
            throw new NotImplementedException();
        }
    }
}
