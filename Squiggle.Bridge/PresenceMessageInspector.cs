using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core.Presence.Transport;
using System.Net;
using Squiggle.Core.Presence.Transport.Messages;
using Squiggle.Core;

namespace Squiggle.Bridge
{
    class PresenceMessageInspector
    {
        RouteTable routeTable;
        IPEndPoint bridgePresenceEndPoint;
        IPEndPoint bridgeChatEndPoint;

        public PresenceMessageInspector(RouteTable routeTable, IPEndPoint bridgePresenceEndPoint, IPEndPoint bridgeChatEndPoint)
        {
            this.routeTable = routeTable;
            this.bridgePresenceEndPoint = bridgePresenceEndPoint;
            this.bridgeChatEndPoint = bridgeChatEndPoint;
        }

        public void InspectForeignPresenceMessage(Message message, TargetBridge bridge)
        {
            routeTable.AddRemoteClient(message.Sender.ClientID, bridge);
            ReplaceSenderWithBridgeEndPoints(message);
        }

        public void InspectLocalPresenceMessage(Message message)
        {
            routeTable.AddLocalPresenceEndPoint(message.Sender);
            if (message is PresenceMessage)
                routeTable.AddLocalChatEndPoint(message.Sender.ClientID, ((PresenceMessage)message).ChatEndPoint);
        }

        void ReplaceSenderWithBridgeEndPoints(Message message)
        {
            message.Sender = new SquiggleEndPoint(message.Sender.ClientID, bridgePresenceEndPoint);
            if (message is PresenceMessage)
                ((PresenceMessage)message).ChatEndPoint = bridgeChatEndPoint;
        }
    }
}
