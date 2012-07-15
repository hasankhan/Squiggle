using System;
using System.ServiceModel;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Chat;
using System.Net;
using System.Linq;
using Squiggle.Core;
using Squiggle.Core.Presence;
using System.ServiceModel.Dispatcher;
using System.Collections.Generic;
using Squiggle.Core.Chat.Host;

namespace Squiggle.Bridge
{
    public class PresenceMessageForwardedEventArgs: EventArgs
    {
        public IPEndPoint BridgeEndPoint { get; set; }
        public Message Message {get; set; }
        public SquiggleEndPoint Recipient {get; set; }

        public bool IsBroadcast
        {
            get { return Recipient == null; }
        }

        public PresenceMessageForwardedEventArgs (Message message, IPEndPoint bridgeEdnpoint, SquiggleEndPoint recipient)
	    {
            this.Message = message;
            this.BridgeEndPoint = bridgeEdnpoint;
	        this.Recipient = recipient;
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)] 
    public class BridgeHost: IBridgeHost
    {
        SquiggleBridge bridge;

        public event EventHandler<PresenceMessageForwardedEventArgs> PresenceMessageForwarded = delegate { };

        internal BridgeHost(SquiggleBridge bridge)
        {
            this.bridge = bridge;
        }

        public void ForwardPresenceMessage(SquiggleEndPoint recipient, byte[] message, IPEndPoint bridgeEndPoint)
        {
            var msg = Message.Deserialize(message);
            var args = new PresenceMessageForwardedEventArgs(msg, bridgeEndPoint, recipient);
            PresenceMessageForwarded(this, args);
        }

        public void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.GetSessionInfo(sessionId, s, r), sender, recipient);
        }

        public void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SessionInfo sessionInfo)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveSessionInfo(sessionId, s, r, sessionInfo), sender, recipient);
        }

        public void Buzz(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.Buzz(sessionId, s, r), sender, recipient);
        }

        public void UserIsTyping(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.UserIsTyping(sessionId, s, r), sender, recipient);
        }

        public void ReceiveMessage(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, string fontName, int fontSize, System.Drawing.Color color, System.Drawing.FontStyle fontStyle, string message)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveMessage(sessionId, s, r, fontName, fontSize, color, fontStyle, message), sender, recipient);
        }

        public void ReceiveChatInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, SquiggleEndPoint[] participants)
        {
            participants = bridge.ConvertChatEndPointsForRecipient(participants, recipient).ToArray();
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveChatInvite(sessionId, s, r, participants), sender, recipient);
        }

        public void JoinChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.JoinChat(sessionId, s, r), sender, recipient);
        }

        public void LeaveChat(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.LeaveChat(sessionId, s, r), sender, recipient);
        }

        public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveAppInvite(sessionId, s, r, appId, appSessionId, metadata), sender, recipient);
        }

        public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient, byte[] chunk)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveAppData(appSessionId, s, r, chunk), sender, recipient);
        }

        public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.AcceptAppInvite(appSessionId, s, r), sender, recipient);
        }

        public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.CancelAppSession(appSessionId, s, r), sender, recipient);
        }
    }
}
