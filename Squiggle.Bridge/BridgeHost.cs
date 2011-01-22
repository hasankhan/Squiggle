using System;
using System.ServiceModel;
using Squiggle.Chat.Services.Presence.Transport;
using Squiggle.Chat.Services.Chat;
using System.Net;
using System.Linq;
using Squiggle.Chat.Services;
using Squiggle.Chat.Services.Presence;

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
        SquiggleBridge bridge;

        public event EventHandler<PresenceMessageForwardedEventArgs> PresenceMessageForwarded = delegate { };

        internal BridgeHost(SquiggleBridge bridge)
        {
            this.bridge = bridge;
        }

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

        public Chat.Services.Presence.UserInfo GetUserInfo(SquiggleEndPoint user)
        {
            UserInfo userInfo = bridge.RoutePresenceMessageToLocalUser((channel, localEndPoint, presenceEndPoint) => channel.GetUserInfo(presenceEndPoint), sender: null, recepient: user);
            if (userInfo != null)
                bridge.AddLocalChatEndPoint(userInfo.ID, userInfo.ChatEndPoint);
            return userInfo;
        }        

        public void ReceivePresenceMessage(SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] message)
        {
            var messageOBj = Message.Deserialize(message);
            bridge.RoutePresenceMessageToLocalUser((channel, localEndPoint, presenceEndPoint)=>channel.SendMessage(messageOBj, localEndPoint, presenceEndPoint), sender, recepient);
        }

        public Chat.Services.Chat.Host.SessionInfo GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            return bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.GetSessionInfo(sessionId, s, r), sender, recepient);
        }

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.Buzz(sessionId, s, r), sender, recepient);
        }

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.UserIsTyping(sessionId, s, r), sender, recepient);
        }

        public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveMessage(sessionId, s, r, fontName, fontSize, color, fontStyle, message), sender, recepient);
        }

        public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SquiggleEndPoint[] participants)
        {
            var bridgeParticipants = participants.Select(p => new SquiggleEndPoint(p.ClientID, bridge.BridgeEndPointRemote)).ToArray();
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveChatInvite(sessionId, s, r, bridgeParticipants), sender, recepient);
        }

        public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.JoinChat(sessionId, s, r), sender, recepient);
        }

        public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.LeaveChat(sessionId, s, r), sender, recepient);
        }

        public void ReceiveFileInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid id, string name, long size)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveFileInvite(sessionId, s, r, id, name, size), sender, recepient);
        }

        public void ReceiveFileContent(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveFileContent(id, s, r, chunk), sender, recepient);
        }

        public void AcceptFileInvite(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.AcceptFileInvite(id, s, r), sender, recepient);
        }

        public void CancelFileTransfer(Guid id, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.CancelFileTransfer(id, s, r), sender, recepient);
        }        
    }
}
