using System;
namespace Squiggle.Client
{
    public interface IBuddy
    {
        string Id { get; }
        string DisplayName { get; }
        DateTime LastUpdated { get; }
        Squiggle.Core.Presence.UserStatus Status { get; }
        event EventHandler Offline;
        event EventHandler Online;
        Squiggle.Core.Presence.IBuddyProperties Properties { get; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
