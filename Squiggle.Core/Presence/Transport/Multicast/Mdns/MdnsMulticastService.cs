using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Makaretu.Dns;
using Squiggle.Core.Presence.Transport.Messages;
using Squiggle.Utilities;
using PresenceMessage = Squiggle.Core.Presence.Transport.Message;

namespace Squiggle.Core.Presence.Transport.Multicast.Mdns
{
    /// <summary>
    /// mDNS/DNS-SD based peer discovery implementing IMulticastService.
    /// Advertises this peer as a _squiggle._tcp.local service and discovers
    /// other peers via DNS-SD service browsing.
    /// </summary>
    class MdnsMulticastService : IMulticastService, IDisposable
    {
        static readonly DomainName ServiceType = new DomainName("_squiggle._tcp");

        readonly IPEndPoint localEndPoint;

        MulticastService? mdns;
        ServiceDiscovery? serviceDiscovery;
        ServiceProfile? serviceProfile;

        readonly ConcurrentDictionary<string, DiscoveredPeer> discoveredPeers = new();

        Timer? expiryTimer;
        static readonly TimeSpan PeerTtl = TimeSpan.FromSeconds(120);
        static readonly TimeSpan ExpiryCheckInterval = TimeSpan.FromSeconds(30);

        bool started;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived = delegate { };

        // Locally advertised state
        string? advertisedClientId;
        string? advertisedDisplayName;
        int advertisedStatus;
        string? advertisedChatIp;
        int advertisedChatPort;
        string? advertisedPresIp;
        int advertisedPresPort;
        TimeSpan advertisedKeepAlive;
        IDictionary<string, string>? advertisedProperties;

        public MdnsMulticastService(IPEndPoint localEndPoint)
        {
            this.localEndPoint = localEndPoint;
        }

        public void Start()
        {
            if (started) return;
            started = true;

            mdns = new MulticastService();
            serviceDiscovery = new ServiceDiscovery(mdns);

            serviceDiscovery.ServiceInstanceDiscovered += OnServiceInstanceDiscovered;
            serviceDiscovery.ServiceInstanceShutdown += OnServiceInstanceShutdown;

            mdns.NetworkInterfaceDiscovered += (s, e) =>
            {
                // Browse for other Squiggle peers
                serviceDiscovery.QueryServiceInstances(ServiceType);
            };

            mdns.Start();

            expiryTimer = new Timer(_ => CheckExpiredPeers(), null, ExpiryCheckInterval, ExpiryCheckInterval);
        }

        public void Stop()
        {
            if (!started) return;
            started = false;

            expiryTimer?.Dispose();
            expiryTimer = null;

            if (serviceProfile != null && serviceDiscovery != null)
            {
                ExceptionMonster.EatTheException(() =>
                {
                    serviceDiscovery.Unadvertise(serviceProfile);
                }, "unadvertising mDNS service");
                serviceProfile = null;
            }

            if (serviceDiscovery != null)
            {
                serviceDiscovery.ServiceInstanceDiscovered -= OnServiceInstanceDiscovered;
                serviceDiscovery.ServiceInstanceShutdown -= OnServiceInstanceShutdown;
                serviceDiscovery.Dispose();
                serviceDiscovery = null;
            }

            if (mdns != null)
            {
                mdns.Stop();
                mdns.Dispose();
                mdns = null;
            }

            // Fire logout for all tracked peers
            foreach (var kvp in discoveredPeers)
            {
                if (discoveredPeers.TryRemove(kvp.Key, out var peer))
                    FireLogout(peer);
            }
        }

        public void SendMessage(PresenceMessage message)
        {
            if (!started || serviceDiscovery == null || mdns == null) return;

            if (message is LoginMessage)
            {
                AdvertiseSelf(message.Sender);
            }
            else if (message is LogoutMessage)
            {
                if (serviceProfile != null)
                {
                    ExceptionMonster.EatTheException(() =>
                    {
                        serviceDiscovery.Unadvertise(serviceProfile);
                    }, "unadvertising mDNS service on logout");
                    serviceProfile = null;
                }
            }
            else if (message is UserUpdateMessage)
            {
                // Re-advertise with updated TXT records
                AdvertiseSelf(message.Sender);
            }
        }

        void AdvertiseSelf(SquiggleEndPoint sender)
        {
            if (serviceDiscovery == null) return;

            // Unadvertise old profile if exists
            if (serviceProfile != null)
            {
                ExceptionMonster.EatTheException(() =>
                {
                    serviceDiscovery.Unadvertise(serviceProfile);
                }, "unadvertising old mDNS profile");
            }

            advertisedClientId = sender.ClientID;
            advertisedPresIp = sender.Address.Address.ToString();
            advertisedPresPort = sender.Address.Port;

            var instanceName = $"squiggle-{sender.ClientID}";
            serviceProfile = new ServiceProfile(instanceName, ServiceType, (ushort)localEndPoint.Port);

            serviceProfile.AddProperty("id", advertisedClientId ?? "");
            serviceProfile.AddProperty("presip", advertisedPresIp ?? "");
            serviceProfile.AddProperty("presport", advertisedPresPort.ToString());

            if (advertisedDisplayName != null)
                serviceProfile.AddProperty("name", advertisedDisplayName);
            serviceProfile.AddProperty("status", advertisedStatus.ToString());

            if (advertisedChatIp != null)
            {
                serviceProfile.AddProperty("chatip", advertisedChatIp);
                serviceProfile.AddProperty("chatport", advertisedChatPort.ToString());
            }

            serviceProfile.AddProperty("keepalive", ((int)advertisedKeepAlive.TotalSeconds).ToString());

            if (advertisedProperties != null)
            {
                foreach (var kvp in advertisedProperties)
                    serviceProfile.AddProperty($"p_{kvp.Key}", kvp.Value);
            }

            serviceDiscovery.Advertise(serviceProfile);
            serviceDiscovery.Announce(serviceProfile);
        }

        /// <summary>
        /// Update locally cached user info that will be included in mDNS TXT records.
        /// </summary>
        public void UpdateAdvertisedInfo(IUserInfo user)
        {
            advertisedClientId = user.ID;
            advertisedDisplayName = user.DisplayName;
            advertisedStatus = (int)user.Status;
            advertisedChatIp = user.ChatEndPoint.Address.ToString();
            advertisedChatPort = user.ChatEndPoint.Port;
            advertisedPresIp = user.PresenceEndPoint.Address.ToString();
            advertisedPresPort = user.PresenceEndPoint.Port;
            advertisedKeepAlive = user.KeepAliveSyncTime;
            advertisedProperties = user.Properties;
        }

        void OnServiceInstanceDiscovered(object? sender, ServiceInstanceDiscoveryEventArgs e)
        {
            if (!started) return;

            var serviceName = e.ServiceInstanceName.ToString();
            if (!serviceName.Contains("_squiggle._tcp")) return;

            // Extract TXT records from the discovery message
            var txtRecords = e.Message.Answers
                .OfType<TXTRecord>()
                .Concat(e.Message.AdditionalRecords.OfType<TXTRecord>());

            foreach (var txt in txtRecords)
            {
                if (txt.Name.ToString().Contains("_squiggle._tcp"))
                    ProcessTxtRecord(txt, serviceName);
            }

            // If no TXT found in this message, query for full records
            if (!txtRecords.Any())
            {
                ExceptionMonster.EatTheException(() =>
                {
                    mdns?.SendQuery(e.ServiceInstanceName, type: DnsType.ANY);
                }, "querying mDNS service instance details");
            }
        }

        void OnServiceInstanceShutdown(object? sender, ServiceInstanceShutdownEventArgs e)
        {
            if (!started) return;

            var serviceName = e.ServiceInstanceName.ToString();
            if (!serviceName.Contains("_squiggle._tcp")) return;

            HandleServiceGoodbye(serviceName);
        }

        void ProcessTxtRecord(TXTRecord txtRecord, string serviceName)
        {
            var props = new Dictionary<string, string>();
            foreach (var str in txtRecord.Strings)
            {
                var eqIdx = str.IndexOf('=');
                if (eqIdx > 0)
                    props[str.Substring(0, eqIdx)] = str.Substring(eqIdx + 1);
            }

            if (!props.TryGetValue("id", out var clientId) || string.IsNullOrEmpty(clientId))
                return;

            // Skip our own advertisement
            if (clientId == advertisedClientId)
                return;

            if (!props.TryGetValue("presip", out var presIp) || !props.TryGetValue("presport", out var presPortStr))
                return;

            if (!IPAddress.TryParse(presIp, out var presAddr) || !int.TryParse(presPortStr, out var presPort))
                return;

            var presEndPoint = new IPEndPoint(presAddr, presPort);

            props.TryGetValue("name", out var displayName);
            int.TryParse(props.GetValueOrDefault("status", "0"), out var status);
            props.TryGetValue("chatip", out var chatIp);
            int.TryParse(props.GetValueOrDefault("chatport", "0"), out var chatPort);
            int.TryParse(props.GetValueOrDefault("keepalive", "60"), out var keepAliveSecs);

            IPEndPoint? chatEndPoint = null;
            if (!string.IsNullOrEmpty(chatIp) && IPAddress.TryParse(chatIp, out var chatAddr))
                chatEndPoint = new IPEndPoint(chatAddr, chatPort);

            var userProps = new Dictionary<string, string>();
            foreach (var kvp in props)
            {
                if (kvp.Key.StartsWith("p_"))
                    userProps[kvp.Key.Substring(2)] = kvp.Value;
            }

            var isNew = !discoveredPeers.ContainsKey(clientId);

            discoveredPeers.AddOrUpdate(clientId,
                _ => new DiscoveredPeer
                {
                    ClientID = clientId,
                    DisplayName = displayName ?? "",
                    Status = (UserStatus)status,
                    PresenceEndPoint = presEndPoint,
                    ChatEndPoint = chatEndPoint,
                    KeepAliveSyncTime = TimeSpan.FromSeconds(keepAliveSecs),
                    Properties = userProps,
                    LastSeen = DateTime.UtcNow,
                    ServiceName = serviceName
                },
                (_, existing) =>
                {
                    existing.DisplayName = displayName ?? existing.DisplayName;
                    existing.Status = (UserStatus)status;
                    existing.PresenceEndPoint = presEndPoint;
                    existing.ChatEndPoint = chatEndPoint ?? existing.ChatEndPoint;
                    existing.KeepAliveSyncTime = TimeSpan.FromSeconds(keepAliveSecs);
                    existing.Properties = userProps;
                    existing.LastSeen = DateTime.UtcNow;
                    existing.ServiceName = serviceName;
                    return existing;
                });

            if (isNew)
                FireLogin(clientId, presEndPoint);
            else
                FireUpdate(clientId, presEndPoint);
        }

        void HandleServiceGoodbye(string serviceName)
        {
            foreach (var kvp in discoveredPeers)
            {
                if (kvp.Value.ServiceName == serviceName)
                {
                    if (discoveredPeers.TryRemove(kvp.Key, out var peer))
                        FireLogout(peer);
                    break;
                }
            }
        }

        void CheckExpiredPeers()
        {
            if (!started) return;

            var now = DateTime.UtcNow;
            foreach (var kvp in discoveredPeers)
            {
                if (now - kvp.Value.LastSeen > PeerTtl)
                {
                    if (discoveredPeers.TryRemove(kvp.Key, out var peer))
                        FireLogout(peer);
                }
            }

            // Re-query periodically
            ExceptionMonster.EatTheException(() =>
            {
                serviceDiscovery?.QueryServiceInstances(ServiceType);
            }, "periodic mDNS query");
        }

        void FireLogin(string clientId, IPEndPoint presEndPoint)
        {
            var message = new LoginMessage
            {
                Sender = new SquiggleEndPoint(clientId, presEndPoint)
            };
            MessageReceived(this, new MessageReceivedEventArgs { Message = message });
        }

        void FireLogout(DiscoveredPeer peer)
        {
            var message = new LogoutMessage
            {
                Sender = new SquiggleEndPoint(peer.ClientID, peer.PresenceEndPoint)
            };
            MessageReceived(this, new MessageReceivedEventArgs { Message = message });
        }

        void FireUpdate(string clientId, IPEndPoint presEndPoint)
        {
            var message = new UserUpdateMessage
            {
                Sender = new SquiggleEndPoint(clientId, presEndPoint)
            };
            MessageReceived(this, new MessageReceivedEventArgs { Message = message });
        }

        public void Dispose()
        {
            Stop();
        }

        class DiscoveredPeer
        {
            public string ClientID { get; set; } = "";
            public string DisplayName { get; set; } = "";
            public UserStatus Status { get; set; }
            public IPEndPoint PresenceEndPoint { get; set; } = null!;
            public IPEndPoint? ChatEndPoint { get; set; }
            public TimeSpan KeepAliveSyncTime { get; set; }
            public IDictionary<string, string>? Properties { get; set; }
            public DateTime LastSeen { get; set; }
            public string ServiceName { get; set; } = "";
        }
    }
}
