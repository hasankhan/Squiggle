using System;
using System.Collections.Generic;
namespace Squiggle.Core.Presence
{
    public interface IBuddyProperties
    {
        byte[] DisplayImage { get; set; }
        string DisplayMessage { get; set; }
        string EmailAddress { get; set; }
        string GroupName { get; set; }
        string MachineName { get; set; }
        event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        string this[string key] { get; set; }
        IDictionary<string, string> ToDictionary();
        IBuddyProperties Clone();
    }
}
