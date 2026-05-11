using System;
using System.Net;
using Squiggle.Core.Presence;

namespace Squiggle.Client
{
    public record LoginOptions
    {
        public required IPEndPoint ChatEndPoint { get; init; }
        public required IPEndPoint MulticastEndPoint { get; init; }
        public required IPEndPoint MulticastReceiveEndPoint { get; init; }
        public required IPEndPoint PresenceServiceEndPoint { get; init; }
        public TimeSpan KeepAliveTime { get; init; }
        public required string DisplayName { get; init; }
        public required IBuddyProperties UserProperties { get; init; }
    }
}
