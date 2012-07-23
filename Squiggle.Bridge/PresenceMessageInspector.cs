using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Squiggle.Core;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Presence.Transport.Messages;

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

        public void InspectForeignPresenceMessage(Squiggle.Core.Presence.Transport.Message message, IPEndPoint bridge)
        {
            routeTable.AddRemoteClient(message.Sender.ClientID, bridge);
            ReplaceSenderWithBridgeEndPoints(message);
        }

        public void InspectLocalPresenceMessage(Squiggle.Core.Presence.Transport.Message message)
        {
            routeTable.AddLocalPresenceEndPoint(message.Sender);
            if (message is PresenceMessage)
                routeTable.AddLocalChatEndPoint(message.Sender.ClientID, ((PresenceMessage)message).ChatEndPoint);
        }

        void ReplaceSenderWithBridgeEndPoints(Squiggle.Core.Presence.Transport.Message message)
        {
            message.Sender = new SquiggleEndPoint(message.Sender.ClientID, bridgePresenceEndPoint);
            if (message is PresenceMessage)
                ((PresenceMessage)message).ChatEndPoint = bridgeChatEndPoint;
        }
    }
}
