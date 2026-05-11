namespace Squiggle.Core.Presence
{
    public enum DiscoveryMode
    {
        /// <summary>
        /// Standard mDNS/DNS-SD discovery using _squiggle._tcp.local
        /// </summary>
        Mdns,

        /// <summary>
        /// Legacy UDP multicast discovery
        /// </summary>
        UdpMulticast,

        /// <summary>
        /// Legacy TCP relay discovery via MulticastServer
        /// </summary>
        TcpMulticast
    }
}
