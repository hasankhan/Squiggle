using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.Linq;
using Squiggle.Chat.Services.Presence.Transport;
using System.ServiceModel.Channels;
using Squiggle.Chat.Services.Presence.Transport.Host;
using Squiggle.Chat.Services;
using Squiggle.Chat.Services.Chat.Host;

namespace Squiggle.Bridge
{
    class TargetBridge
    {
        public IPEndPoint EndPoint { get; set; }
        public BridgeHostProxy Proxy { get; set; }
    }

    class SquiggleBridge
    {
        BridgeHost bridgeHost;
        ServiceHost serviceHost;
        IPEndPoint presenceServiceEndPoint;

        List<TargetBridge> targetBridges = new List<TargetBridge>();
        Dictionary<string, TargetBridge> remoteClientBridgeMap = new Dictionary<string, TargetBridge>();
        Dictionary<string, SquiggleEndPoint> localPresenceEndPoints = new Dictionary<string, SquiggleEndPoint>();
        Dictionary<string, SquiggleEndPoint> localChatEndPoints = new Dictionary<string, SquiggleEndPoint>();

        public PresenceChannel PresenceChannel { get; private set; }
        public IPEndPoint BridgeEndPointLocal { get; private set; }
        public IPEndPoint BridgeEndPointRemote { get; private set; }

        public SquiggleBridge()
        {
            bridgeHost = new BridgeHost(this);
            bridgeHost.PresenceMessageForwarded += new EventHandler<PresenceMessageForwardedEventArgs>(bridgeHost_PresenceMessageForwarded);
        }

        public void AddTarget(IPEndPoint target)
        {
            Uri address;
            Binding binding;
            GetBridgeConnectionParams(target, out address, out binding);
            var proxy = new BridgeHostProxy(binding, new EndpointAddress(address));
            targetBridges.Add(new TargetBridge()
            {
                EndPoint = target,
                Proxy = proxy
            });
        }        

        public void Start(IPEndPoint bridgeEndPointLocal, IPEndPoint bridgeEndPointRemote, IPEndPoint presenceEndPoint)
        {
            this.BridgeEndPointLocal = bridgeEndPointLocal;
            this.BridgeEndPointRemote = bridgeEndPointRemote;

            Uri address;            
            serviceHost = new ServiceHost(bridgeHost);
            Binding binding;
            GetBridgeConnectionParams(bridgeEndPointLocal, out address, out binding);            
            serviceHost.AddServiceEndpoint(typeof(IBridgeHost), binding, address);
            GetBridgeConnectionParams(bridgeEndPointRemote, out address, out binding);
            serviceHost.AddServiceEndpoint(typeof(IBridgeHost), binding, address);
            
            serviceHost.Open();

            this.presenceServiceEndPoint = new IPEndPoint(bridgeEndPointLocal.Address, presenceEndPoint.Port);
            PresenceChannel = new PresenceChannel(presenceEndPoint, presenceServiceEndPoint);
            PresenceChannel.Start();
            PresenceChannel.MessageReceived += new EventHandler<Chat.Services.Presence.Transport.MessageReceivedEventArgs>(presenceChannel_MessageReceived);
            PresenceChannel.UserInfoRequested += new EventHandler<Chat.Services.Presence.Transport.Host.UserInfoRequestedEventArgs>(presenceChannel_UserInfoRequested);
        }

        public void Stop()
        {
            PresenceChannel.Stop();
            serviceHost.Close();
            foreach (TargetBridge target in targetBridges)
                target.Proxy.Dispose();
        }

        void bridgeHost_PresenceMessageForwarded(object sender, PresenceMessageForwardedEventArgs e)
        {
            if (e.Message.ChannelID != PresenceChannel.ChannelID)
            {
                TargetBridge bridge = FindBridge(e.BridgeEndPoint);
                if (bridge != null)
                {
                    remoteClientBridgeMap[e.Message.ClientID] = bridge;
                    e.Message.PresenceEndPoint = presenceServiceEndPoint;
                    PresenceChannel.SendMessage(e.Message);
                }
                Console.WriteLine(e.Message.ToString());
            }
        }

        void presenceChannel_UserInfoRequested(object sender, UserInfoRequestedEventArgs e)
        {
            TargetBridge bridge = FindBridge(e.User.ClientID);
            if (bridge != null)
                e.UserInfo = bridge.Proxy.GetUserInfo(e.User);
        }

        void presenceChannel_MessageReceived(object sender, Squiggle.Chat.Services.Presence.Transport.MessageReceivedEventArgs e)
        {
            localPresenceEndPoints[e.Sender.ClientID] = e.Sender;
            e.Sender.Address = BridgeEndPointRemote;
            byte[] message = e.Message.Serialize();

            if (e.IsBroadcast)
            {
                foreach (TargetBridge target in targetBridges)
                    target.Proxy.ForwardPresenceMessage(message, BridgeEndPointRemote);
            }
            else
            {
                TargetBridge bridge = FindBridge(e.Recipient.ClientID);
                bridge.Proxy.ReceivePresenceMessage(e.Sender, e.Recipient, message);
            }
        }        

        public void RoutePresenceMessageToLocalUser(Action<PresenceChannel, SquiggleEndPoint, SquiggleEndPoint> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            RoutePresenceMessageToLocalUser((channel, localEndPoint, presenceEndPoint) =>
            {
                action(channel, localEndPoint, presenceEndPoint);
                return (object)null;
            }, sender, recepient);
        }

        public T RoutePresenceMessageToLocalUser<T>(Func<PresenceChannel, SquiggleEndPoint, SquiggleEndPoint, T> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            SquiggleEndPoint endPoint;
            localPresenceEndPoints.TryGetValue(recepient.ClientID, out endPoint);
            if (endPoint != null)
            {
                recepient = new SquiggleEndPoint(recepient.ClientID, endPoint.Address);
                if (sender != null)
                    sender = new SquiggleEndPoint(sender.ClientID, presenceServiceEndPoint);
                return action(PresenceChannel, sender, recepient);
            }
            return default(T);
        }

        public void AddLocalChatEndPoint(string clientID, IPEndPoint endPoint)
        {
            localChatEndPoints[clientID] = new SquiggleEndPoint(clientID, endPoint);
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
                return RouteChatMessageToRemoteUser(action, sender, recepient);
        }

        bool RecipientIsLocal(SquiggleEndPoint recepient)
        {
            return localChatEndPoints.ContainsKey(recepient.ClientID);
        }

        T RouteChatMessageToRemoteUser<T>(Func<IChatHost, SquiggleEndPoint, SquiggleEndPoint, T> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            sender = new SquiggleEndPoint(sender.ClientID, BridgeEndPointRemote);
            TargetBridge bridge = FindBridge(recepient.ClientID);
            if (bridge != null)
                return action(bridge.Proxy, sender, recepient);
            return default(T);
        }

        T RouteChatMessageToLocalUser<T>(Func<IChatHost, SquiggleEndPoint, SquiggleEndPoint, T> action, SquiggleEndPoint sender, SquiggleEndPoint recepient)
        {
            sender = new SquiggleEndPoint(sender.ClientID, BridgeEndPointLocal);
            SquiggleEndPoint endPoint;
            localChatEndPoints.TryGetValue(recepient.ClientID, out endPoint);
            var proxy = ChatHostProxyFactory.Get(recepient.Address);
            return action(proxy, sender, endPoint);
        }

        TargetBridge FindBridge(string clientID)
        {
            TargetBridge bridge;
            remoteClientBridgeMap.TryGetValue(clientID, out bridge);
            return bridge;
        }

        TargetBridge FindBridge(IPEndPoint bridgeEndPoint)
        {
            TargetBridge bridge = targetBridges.FirstOrDefault(t => t.EndPoint.Equals(bridgeEndPoint));
            return bridge;
        }

        void GetBridgeConnectionParams(IPEndPoint endPoint, out Uri address, out Binding binding)
        {
            address = new Uri("net.tcp://" + endPoint.ToString() + "/squigglebridge");
            binding = new NetTcpBinding(SecurityMode.None);
        }
    }
}
