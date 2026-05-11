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
    }
}
