using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Core.Presence.Transport;
using System.ServiceModel.Channels;
using Squiggle.Core.Presence.Transport.Host;
using Squiggle.Core;
using Squiggle.Core.Chat.Transport.Host;
using System.Diagnostics;
using Squiggle.Utilities;
using Squiggle.Core.Presence.Transport.Messages;

namespace Squiggle.Bridge
{
    class TargetBridge
    {
        public IPEndPoint EndPoint { get; set; }
        public BridgeHostProxy Proxy { get; set; }
    }

    delegate void RouteAction(IChatHost targetHost, SquiggleEndPoint sender, SquiggleEndPoint recipient);

    class SquiggleBridge: WcfHost
    {
        BridgeHost bridgeHost;
        IPEndPoint presenceServiceEndPoint;
        IPEndPoint bridgeEndPointInternal;
        IPEndPoint bridgeEndPointExternal;
        IPEndPoint multicastEndPoint;
        PresenceChannel presenceChannel;
        PresenceMessageInspector messageInspector;

        List<TargetBridge> targetBridges = new List<TargetBridge>();
        RouteTable routeTable;


        public SquiggleBridge(IPEndPoint bridgeEndPointInternal,
                              IPEndPoint bridgeEndPointExternal,
                              IPEndPoint multicastEndPoint,
                              IPEndPoint presenceServiceEndPoint)
        {
            this.bridgeEndPointInternal = bridgeEndPointInternal;
            this.bridgeEndPointExternal = bridgeEndPointExternal;
            this.presenceServiceEndPoint = presenceServiceEndPoint;
            this.multicastEndPoint = multicastEndPoint;

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

            routeTable = new RouteTable();
            messageInspector = new PresenceMessageInspector(routeTable, presenceServiceEndPoint, bridgeEndPointInternal);

            presenceChannel = new PresenceChannel(multicastEndPoint, presenceServiceEndPoint);
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

            messageInspector.InspectForeignPresenceMessage(e.Message, bridge);

            Trace.WriteLine("Replay: " + e.Message.GetType().Name);

            if (e.IsBroadcast)
                ExceptionMonster.EatTheException(() =>
                {
                    presenceChannel.BroadcastMessage(e.Message);
                }, "replaying presence message to local clients");
            else
                ExceptionMonster.EatTheException(() =>
                {
                    IPEndPoint endpoint = routeTable.GetLocalPresenceEndPoint(e.Recipient.ClientID);

                    if (endpoint != null)
                    {
                        var recipient = new SquiggleEndPoint(e.Recipient.ClientID, endpoint);
                        presenceChannel.SendMessage(e.Message, recipient);
                    }
                }, "routing presence message to local user");
        }

        void presenceChannel_MessageReceived(object sender, Squiggle.Core.Presence.Transport.MessageReceivedEventArgs e)
        {
            ExceptionMonster.EatTheException(() =>
            {
                messageInspector.InspectLocalPresenceMessage(e.Message);

                byte[] message = e.Message.Serialize();

                if (e.IsBroadcast)
                {
                    foreach (TargetBridge target in targetBridges)
                        target.Proxy.ForwardPresenceMessage(null, message, bridgeEndPointExternal);
                }
                else
                {
                    TargetBridge bridge = routeTable.FindBridge(e.Recipient.ClientID);
                    bridge.Proxy.ForwardPresenceMessage(e.Recipient, message, bridgeEndPointExternal);
                }
                Trace.WriteLine("Forward: " + e.Message.GetType().Name);
            }, "forwarding presence message to bridge(s)");
        }       

        public void RouteChatMessageToLocalOrRemoteUser(RouteAction action, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            if (IsLocalChatEndpoint(recipient))
                RouteChatMessageToLocalUser(action, sender, recipient);
            else
                RouteMessageToRemoteUser((h, s, r) => action(h, s, r), sender, recipient);
        }

        public IEnumerable<SquiggleEndPoint> ConvertChatEndPointsForRecipient(IEnumerable<SquiggleEndPoint> endpoints, SquiggleEndPoint recipient)
        {
            if (IsLocalChatEndpoint(recipient))
                return endpoints.Select(ep => new SquiggleEndPoint(ep.ClientID, bridgeEndPointInternal)).ToList();
            return endpoints;
        }

        bool IsLocalChatEndpoint(SquiggleEndPoint recipient)
        {
            return routeTable.GetLocalChatEndPoint(recipient.ClientID) != null;
        }

        void RouteMessageToRemoteUser(RouteAction action, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            ExceptionMonster.EatTheException(() =>
            {
                TargetBridge bridge = routeTable.FindBridge(recipient.ClientID);
                if (bridge != null)
                    action(bridge.Proxy, sender, recipient);                
            }, "routing message to remote user");            
        }

        void RouteChatMessageToLocalUser(RouteAction action, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            ExceptionMonster.EatTheException(() =>
            {
                sender = new SquiggleEndPoint(sender.ClientID, bridgeEndPointInternal);
                IPEndPoint endpoint = routeTable.GetLocalChatEndPoint(recipient.ClientID);
                var proxy = ChatHostProxyFactory.Get(endpoint);
                action(proxy, sender, new SquiggleEndPoint(recipient.ClientID, endpoint));
            }, "routing chat message to local user");            
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
            GetBridgeConnectionParams(bridgeEndPointExternal, ServiceNames.BridgeService, out address, out binding);
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
