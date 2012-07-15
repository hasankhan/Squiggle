using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squiggle.Core;
using System.Net;

namespace Squiggle.Bridge
{
    class RouteTable
    {
        Dictionary<string, TargetBridge> remoteClientBridgeMap = new Dictionary<string, TargetBridge>();
        Dictionary<string, IPEndPoint> localPresenceEndPoints = new Dictionary<string, IPEndPoint>();
        Dictionary<string, IPEndPoint> localChatEndPoints = new Dictionary<string, IPEndPoint>();

        public void AddRemoteClient(string clientId, TargetBridge bridge)
        {
            lock (remoteClientBridgeMap)
                remoteClientBridgeMap[clientId] = bridge;
        }

        public IPEndPoint GetLocalPresnceEndPoint(string clientId)
        {
            IPEndPoint endpoint;
            lock (localPresenceEndPoints)
                localPresenceEndPoints.TryGetValue(clientId, out endpoint);
            return endpoint;
        }

        public void AddLocalPresenceEndPoint(SquiggleEndPoint localPresenceEndPoint)
        {
            lock (localPresenceEndPoints)
                localPresenceEndPoints[localPresenceEndPoint.ClientID] = localPresenceEndPoint.Address;
        }

        public void AddLocalChatEndPoint(string clientId, IPEndPoint endpoint)
        {
            lock (localChatEndPoints)
                localChatEndPoints[clientId] = endpoint;
        }

        public IPEndPoint GetLocalChatEndPoint(string clientId)
        {
            IPEndPoint endpoint;
            lock (localChatEndPoints)
                localChatEndPoints.TryGetValue(clientId, out endpoint);
            return endpoint;
        }

        public TargetBridge FindBridge(string clientID)
        {
            TargetBridge bridge;
            lock (remoteClientBridgeMap)
                remoteClientBridgeMap.TryGetValue(clientID, out bridge);
            return bridge;
        }
    }
}
