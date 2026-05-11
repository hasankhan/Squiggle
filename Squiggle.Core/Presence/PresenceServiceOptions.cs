using System;
using System.Net;

namespace Squiggle.Core.Presence
{
    public record PresenceServiceOptions
    {
        public required SquiggleEndPoint ChatEndPoint { get; init; }
        public required IPEndPoint MulticastEndPoint { get; init; }
        public required IPEndPoint MulticastReceiveEndPoint { get; init; }
        public required IPEndPoint PresenceServiceEndPoint { get; init; }
        public TimeSpan KeepAliveTime { get; init; }

        /// <summary>
        /// Discovery mode. Defaults to Mdns. Use UdpMulticast or TcpMulticast for legacy behavior.
        /// </summary>
        public DiscoveryMode DiscoveryMode { get; init; } = DiscoveryMode.Mdns;
    }
}
