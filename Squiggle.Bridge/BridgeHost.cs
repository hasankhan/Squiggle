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
        public SquiggleEndPoint Recepient {get; set; }

        public bool IsBroadcast
        {
            get { return Recepient == null; }
        }

        public PresenceMessageForwardedEventArgs (Message message, IPEndPoint bridgeEdnpoint, SquiggleEndPoint recepient)
	    {
            this.Message = message;
            this.BridgeEndPoint = bridgeEdnpoint;
	        this.Recepient = recepient;
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

        public void ForwardPresenceMessage(SquiggleEndPoint recepient, byte[] message, IPEndPoint bridgeEndPoint)
        {
            var msg = Message.Deserialize(message);
            var args = new PresenceMessageForwardedEventArgs(msg, bridgeEndPoint, recepient);
            PresenceMessageForwarded(this, args);
        }

        public void GetSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.GetSessionInfo(sessionId, s, r), sender, recepient);
        }

        public void ReceiveSessionInfo(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, SessionInfo sessionInfo)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.GetSessionInfo(sessionId, s, r), sender, recepient);
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
            var bridgeParticipants = participants.Select(p => new SquiggleEndPoint(p.ClientID, bridge.BridgeEndPointExternal)).ToArray();
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

        public void ReceiveAppInvite(Guid sessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, Guid appId, Guid appSessionId, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveAppInvite(sessionId, s, r, appId, appSessionId, metadata), sender, recepient);
        }

        public void ReceiveAppData(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient, byte[] chunk)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.ReceiveAppData(appSessionId, s, r, chunk), sender, recepient);
        }

        public void AcceptAppInvite(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.AcceptAppInvite(appSessionId, s, r), sender, recepient);
        }

        public void CancelAppSession(Guid appSessionId, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            bridge.RouteChatMessageToLocalOrRemoteUser((h, s, r) => h.CancelAppSession(appSessionId, s, r), sender, recepient);
        }
    }
}
