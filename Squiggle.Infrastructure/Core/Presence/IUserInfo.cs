using System;
namespace Squiggle.Core.Presence
{
    public interface IUserInfo
    {
        System.Net.IPEndPoint ChatEndPoint { get; set; }
        string DisplayName { get; set; }
        string ID { get; set; }
        TimeSpan KeepAliveSyncTime { get; set; }
        System.Net.IPEndPoint PresenceEndPoint { get; set; }
        System.Collections.Generic.IDictionary<string, string> Properties { get; set; }
        UserStatus Status { get; set; }
        void Update(IUserInfo user);
    }
}
