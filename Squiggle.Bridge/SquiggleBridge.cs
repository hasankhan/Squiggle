using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Core.Presence.Transport;
using System.ServiceModel.Channels;
using Squiggle.Core.Presence.Transport.Host;
using Squiggle.Core;
using Squiggle.Core.Chat.Host;
using System.Diagnostics;
using Squiggle.Utilities;

namespace Squiggle.Bridge
{
    class TargetBridge
    {
        public IPEndPoint EndPoint { get; set; }
        public BridgeHostProxy Proxy { get; set; }
    }

    class SquiggleBridge: WcfHost
    {
        BridgeHost bridgeHost;
        IPEndPoint presenceServiceEndPoint;
        IPEndPoint bridgeEndPointInternal;
        IPEndPoint presenceEndPoint;
        PresenceChannel presenceChannel;

        List<TargetBridge> targetBridges = new List<TargetBridge>();
        Dictionary<string, TargetBridge> remoteClientBridgeMap = new Dictionary<string, TargetBridge>();
        Dictionary<string, SquiggleEndPoint> localPresenceEndPoints = new Dictionary<string, SquiggleEndPoint>();
        Dictionary<string, SquiggleEndPoint> localChatEndPoints = new Dictionary<string, SquiggleEndPoint>();

        public IPEndPoint BridgeEndPointExternal { get; private set; }

        public SquiggleBridge(IPEndPoint bridgeEndPointInternal,
                              IPEndPoint bridgeEndPointExternal,
                              IPEndPoint presenceEndPoint,
                              IPEndPoint presenceServiceEndPoint)
        {
            this.bridgeEndPointInternal = bridgeEndPointInternal;
            this.BridgeEndPointExternal = bridgeEndPointExternal;
            this.presenceServiceEndPoint = presenceServiceEndPoint;
            this.presenceEndPoint = presenceEndPoint;

            bridgeHost = new BridgeHost(this);
            bridgeHost.PresenceMessageForwarded += new EventHandler<PresenceMessageForwardedEventArgs>(bridgeHost_PresenceMessageForwarded);
        }

        public void AddTarget(IPEndPoint target)
        {
            Uri address;
            Binding binding;
            GetBridgeConnectionParams(target, ServiceNames.BridgeService, out address, out binding);
            var proxy = new BridgeHostProxy(binding, new EndpointAddress(address));
            targetBridges.Add(new TargetBridge()
            {
                EndPoint = target,
                Proxy = proxy
            });
        }

        protected override void OnStart()
        {
            base.OnStart();

            presenceChannel = new PresenceChannel(presenceEndPoint, presenceServiceEndPoint);
            presenceChannel.Start();
            presenceChannel.MessageReceived += new EventHandler<Squiggle.Core.Presence.Transport.MessageReceivedEventArgs>(presenceChannel_MessageReceived);
        }

        protected override void OnStop()
        {
            base.OnStop();

            presenceChannel.Stop();
            foreach (TargetBridge target in targetBridges)
                target.Proxy.Dispose();
        }

        void bridgeHost_PresenceMessageForwarded(object sender, PresenceMessageForwardedEventArgs e)
        {
            if (e.Message.ChannelID == presenceChannel.ChannelID)
                return; // my own message
            
            TargetBridge bridge = FindBridge(e.BridgeEndPoint);
            if (bridge == null) // not coming from a target bridge list
                return;

            lock (remoteClientBridgeMap)
                remoteClientBridgeMap[e.Message.Sender.ClientID] = bridge;
            e.Message.Sender = new SquiggleEndPoint(e.Message.Sender.ClientID, presenceServiceEndPoint);

            Trace.WriteLine("Replay: " + e.Message.GetType().Name);

            if (e.IsBroadcast)
                ExceptionMonster.EatTheException(() =>
                {
                    presenceChannel.BroadcastMessage(e.Message);
                }, "replaying presence message to local clients");
            else
                ExceptionMonster.EatTheException(() =>
                {
                    SquiggleEndPoint endPoint;
                    lock (localPresenceEndPoints)
                        localPresenceEndPoints.TryGetValue(e.Recepient.ClientID, out endPoint);

                    if (endPoint != null)
                    {
                        var recepient = new SquiggleEndPoint(e.Recepient.ClientID, endPoint.Address);
                        presenceChannel.SendMessage(e.Message, recepient);
                    }
                }, "routing presence message to local user");
        }

        void presenceChannel_MessageReceived(object sender, Squiggle.Core.Presence.Transport.MessageReceivedEventArgs e)
        {
            ExceptionMonster.EatTheException(() =>
            {
                lock (localPresenceEndPoints)
                    localPresenceEndPoints[e.Message.Sender.ClientID] = e.Message.Sender;

                byte[] message = e.Message.Serialize();

                if (e.IsBroadcast)
                {
                    foreach (TargetBridge target in targetBridges)
                        target.Proxy.ForwardPresenceMessage(null, message, BridgeEndPointExternal);
                }
                else
                {
                    TargetBridge bridge = FindBridge(e.Recipient.ClientID);
                    bridge.Proxy.ForwardPresenceMessage(e.Recipient, message, BridgeEndPointExternal);
                }
                Trace.WriteLine("Forward: " + e.Message.GetType().Name);
            }, "forwarding presence message to bridge(s)");
        }        

        public void RouteChatMessageToLocalOrRemoteUser(Action<IChatHost, SquiggleEndPoint, SquiggleEndPoint> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            RouteChatMessageToLocalOrRemoteUser((h,s,e)=>
            {
                action(h, s, e);
                return (object)null;
            }, sender, recepient);
        }

        public T RouteChatMessageToLocalOrRemoteUser<T>(Func<IChatHost, SquiggleEndPoint, SquiggleEndPoint, T> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            if (RecipientIsLocal(recepient))
                return RouteChatMessageToLocalUser(action, sender, recepient);
            else
                return RouteMessageToRemoteUser((h, s, r)=>action(h, s, r), sender, recepient);
        }

        bool RecipientIsLocal(SquiggleEndPoint recepient)
        {
            lock (localChatEndPoints)
                return localChatEndPoints.ContainsKey(recepient.ClientID);
        }

        T RouteMessageToRemoteUser<T>(Func<IChatHost, SquiggleEndPoint, SquiggleEndPoint, T> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            return ExceptionMonster.EatTheException(() =>
            {
                sender = new SquiggleEndPoint(sender.ClientID, BridgeEndPointExternal);
                TargetBridge bridge = FindBridge(recepient.ClientID);
                if (bridge != null)
                    return action(bridge.Proxy, sender, recepient);
                return default(T);
            }, "routing message to remote user");            
        }

        T RouteChatMessageToLocalUser<T>(Func<IChatHost, SquiggleEndPoint, SquiggleEndPoint, T> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            return ExceptionMonster.EatTheException(() =>
            {
                sender = new SquiggleEndPoint(sender.ClientID, bridgeEndPointInternal);
                SquiggleEndPoint endPoint;
                lock (localChatEndPoints)
                    localChatEndPoints.TryGetValue(recepient.ClientID, out endPoint);
                var proxy = ChatHostProxyFactory.Get(recepient.Address);
                return action(proxy, sender, endPoint);
            }, "routing chat message to local user");            
        }

        TargetBridge FindBridge(string clientID)
        {
            TargetBridge bridge;
            lock (remoteClientBridgeMap)
                remoteClientBridgeMap.TryGetValue(clientID, out bridge);
            return bridge;
        }

        TargetBridge FindBridge(IPEndPoint bridgeEndPoint)
        {
            TargetBridge bridge = targetBridges.FirstOrDefault(t => t.EndPoint.Equals(bridgeEndPoint));
            return bridge;
        }        

        protected override ServiceHost CreateHost()
        {
            var serviceHost = new ServiceHost(bridgeHost);

            Binding binding;
            Uri address;
            GetBridgeConnectionParams(bridgeEndPointInternal, ServiceNames.ChatService, out address, out binding);
            serviceHost.AddServiceEndpoint(typeof(IBridgeHost), binding, address);
            GetBridgeConnectionParams(BridgeEndPointExternal, ServiceNames.BridgeService, out address, out binding);
            serviceHost.AddServiceEndpoint(typeof(IBridgeHost), binding, address);

            return serviceHost;
        }

        void GetBridgeConnectionParams(IPEndPoint endPoint, string addressSuffix, out Uri address, out Binding binding)
        {
            address = new Uri("net.tcp://" + endPoint.ToString() + "/" + addressSuffix);
            binding = WcfConfig.CreateBinding();
        }
    }
}
