using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Squiggle.Core;

namespace Squiggle.Bridge
{
    class RouteTable
    {
        Dictionary<string, IPEndPoint> remoteClientBridgeMap = new Dictionary<string, IPEndPoint>();
        Dictionary<string, IPEndPoint> localPresenceEndPoints = new Dictionary<string, IPEndPoint>();
        Dictionary<string, IPEndPoint> localChatEndPoints = new Dictionary<string, IPEndPoint>();

        public void AddRemoteClient(string clientId, IPEndPoint bridge)
        {
            lock (remoteClientBridgeMap)
                remoteClientBridgeMap[clientId] = bridge;
        }

        public IPEndPoint GetLocalPresenceEndPoint(string clientId)
        {
            IPEndPoint endpoint;
            lock (localPresenceEndPoints)
                localPresenceEndPoints.TryGetValue(clientId, out endpoint);
            return endpoint;
        }

        public void AddLocalPresenceEndPoint(ISquiggleEndPoint localPresenceEndPoint)
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

        public IPEndPoint FindBridge(string clientID)
        {
            IPEndPoint bridge;
            lock (remoteClientBridgeMap)
                remoteClientBridgeMap.TryGetValue(clientID, out bridge);
            return bridge;
        }
    }
}
