using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Squiggle.Core;
using Squiggle.Core.Chat.Transport.Host;
using Squiggle.Core.Chat.Transport.Messages;
using Squiggle.Core.Presence.Transport;
using Squiggle.Core.Presence.Transport.Messages;
using Squiggle.Utilities;
using Squiggle.Utilities.Serialization;

namespace Squiggle.Bridge
{
    class TargetBridge
    {
        public IPEndPoint EndPoint { get; set; }
    }

    delegate void RouteAction(bool local, IPEndPoint target, SquiggleEndPoint sender, SquiggleEndPoint recipient);

    class SquiggleBridge
    {
        BridgeHost bridgeHost;
        IPEndPoint presenceServiceEndPoint;
        IPEndPoint bridgeEndPointInternal;
        IPEndPoint bridgeEndPointExternal;
        IPEndPoint multicastEndPoint;
        IPEndPoint multicastReceiveEndPoint;
        PresenceChannel presenceChannel;
        PresenceMessageInspector messageInspector;

        List<IPEndPoint> targetBridges = new List<IPEndPoint>();
        RouteTable routeTable;


        public SquiggleBridge(IPEndPoint bridgeEndPointInternal,
                              IPEndPoint bridgeEndPointExternal,
                              IPEndPoint multicastEndPoint,
                              IPEndPoint multicastReceiveEndPoint,
                              IPEndPoint presenceServiceEndPoint)
        {
            this.bridgeEndPointInternal = bridgeEndPointInternal;
            this.bridgeEndPointExternal = bridgeEndPointExternal;
            this.multicastEndPoint = multicastEndPoint;
            this.multicastReceiveEndPoint = multicastReceiveEndPoint; 
            this.presenceServiceEndPoint = presenceServiceEndPoint;
        }

        public void AddTarget(IPEndPoint target)
        {
            targetBridges.Add(target);
        }

        public void Start()
        {
            bridgeHost = new BridgeHost(this, bridgeEndPointExternal, bridgeEndPointInternal);
            bridgeHost.PresenceMessageForwarded += new EventHandler<PresenceMessageForwardedEventArgs>(bridgeHost_PresenceMessageForwarded);
            bridgeHost.ChatMessageReceived += new EventHandler<ChatMessageReceivedEventArgs>(bridgeHost_ChatMessageReceived);
            bridgeHost.Start();

            routeTable = new RouteTable();
            messageInspector = new PresenceMessageInspector(routeTable, presenceServiceEndPoint, bridgeEndPointInternal);

            presenceChannel = new PresenceChannel(multicastEndPoint, multicastReceiveEndPoint, presenceServiceEndPoint);
            presenceChannel.Start();
            presenceChannel.MessageReceived += new EventHandler<Squiggle.Core.Presence.Transport.MessageReceivedEventArgs>(presenceChannel_MessageReceived);
        }

        public void Stop()
        {
            bridgeHost.Dispose();

            presenceChannel.Stop();
        }

        void bridgeHost_PresenceMessageForwarded(object sender, PresenceMessageForwardedEventArgs e)
        {
            if (e.Message.ChannelID == presenceChannel.ChannelID)
                return; // my own message
            
            if (!targetBridges.Contains(e.BridgeEndPoint)) // not coming from a target bridge list
                return;

            messageInspector.InspectForeignPresenceMessage(e.Message, e.BridgeEndPoint);

            Trace.WriteLine("Replay: " + e.Message.GetType().Name);

            if (e.IsBroadcast)
                ExceptionMonster.EatTheException(() =>
                {
                    presenceChannel.MulticastMessage(e.Message);
                }, "replaying presence message to local clients");
            else
                ExceptionMonster.EatTheException(() =>
                {
                    IPEndPoint endpoint = routeTable.GetLocalPresenceEndPoint(e.Message.Recipient.ClientID);

                    if (endpoint != null)
                    {
                        e.Message.Recipient = new SquiggleEndPoint(e.Message.Recipient.ClientID, endpoint);
                        presenceChannel.SendMessage(e.Message);
                    }
                }, "routing presence message to local user");
        }

        void bridgeHost_ChatMessageReceived(object sender, ChatMessageReceivedEventArgs e)
        {
            if (e.Message is IMessageHasParticipants)
            {
                var msg = (IMessageHasParticipants)e.Message;
                msg.Participants = ConvertChatEndPointsForRecipient(msg.Participants, e.Message.Recipient).ToList();
            }

            RouteChatMessageToLocalOrRemoteUser((local, target, newSender, newRecipient) =>
            {
                e.Message.Sender = newSender;
                e.Message.Recipient = newRecipient;
                bridgeHost.SendChatMessage(local, target, e.Message);
            }, e.Message.Sender, e.Message.Recipient);
        }

        void presenceChannel_MessageReceived(object sender, Squiggle.Core.Presence.Transport.MessageReceivedEventArgs e)
        {
            ExceptionMonster.EatTheException(() =>
            {
                messageInspector.InspectLocalPresenceMessage(e.Message);

                byte[] message = SerializationHelper.Serialize(e.Message);

                if (e.IsBroadcast)
                {
                    foreach (IPEndPoint target in targetBridges)
                        bridgeHost.SendPresenceMessage(target, message);
                }
                else
                {
                    IPEndPoint bridge = routeTable.FindBridge(e.Message.Recipient.ClientID);
                    bridgeHost.SendPresenceMessage(bridge, message);
                }
                Trace.WriteLine("Forward: " + e.Message.GetType().Name);
            }, "forwarding presence message to bridge(s)");
        }       

        void RouteChatMessageToLocalOrRemoteUser(RouteAction action, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            if (IsLocalChatEndpoint(recipient))
                RouteChatMessageToLocalUser(action, sender, recipient);
            else
                RouteMessageToRemoteUser((local, target, s, r) => action(local, target, s, r), sender, recipient);
        }

        IEnumerable<SquiggleEndPoint> ConvertChatEndPointsForRecipient(IEnumerable<SquiggleEndPoint> endpoints, SquiggleEndPoint recipient)
        {
            if (IsLocalChatEndpoint(recipient))
                return endpoints.Select(ep => new SquiggleEndPoint(ep.ClientID, bridgeEndPointInternal)).ToList();
            return endpoints;
        }

        bool IsLocalChatEndpoint(ISquiggleEndPoint recipient)
        {
            return routeTable.GetLocalChatEndPoint(recipient.ClientID) != null;
        }

        void RouteMessageToRemoteUser(RouteAction action, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            ExceptionMonster.EatTheException(() =>
            {
                IPEndPoint bridge = routeTable.FindBridge(recipient.ClientID);
                if (bridge != null)
                    action(false, bridge, sender, recipient);                
            }, "routing message to remote user");            
        }

        void RouteChatMessageToLocalUser(RouteAction action, SquiggleEndPoint sender, SquiggleEndPoint recipient)
        {
            ExceptionMonster.EatTheException(() =>
            {
                sender = new SquiggleEndPoint(sender.ClientID, bridgeEndPointInternal);
                IPEndPoint endpoint = routeTable.GetLocalChatEndPoint(recipient.ClientID);
                action(true, endpoint, sender, new SquiggleEndPoint(recipient.ClientID, endpoint));
            }, "routing chat message to local user");            
        }

        IPEndPoint FindBridge(IPEndPoint bridgeEndPoint)
        {
            IPEndPoint bridge = targetBridges.FirstOrDefault(t => t.Equals(bridgeEndPoint));
            return bridge;
        }    
    }
}
