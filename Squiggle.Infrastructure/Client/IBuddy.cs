using System;
namespace Squiggle.Chat
{
    public interface IBuddy
    {
        System.Net.IPEndPoint ChatEndPoint { get; }
        event EventHandler<ChatStartedEventArgs> ChatStarted;
        string DisplayName { get; set; }
        string Id { get; }
        bool IsOnline { get; }
        DateTime LastUpdated { get; set; }
        event EventHandler Offline;
        event EventHandler Online;
        Squiggle.Core.Presence.IBuddyProperties Properties { get; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        IChat StartChat();
        Squiggle.Core.Presence.UserStatus Status { get; set; }
        void Update(System.Net.IPEndPoint chatEndPoint, System.Collections.Generic.IDictionary<string, string> properties);
    }
}
