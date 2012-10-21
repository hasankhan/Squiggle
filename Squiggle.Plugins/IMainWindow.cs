using System;

namespace Squiggle.Plugins
{
    public interface IMainWindow
    {
        void BlinkTrayIcon();
        void InitializeComponent();
        void Quit();
        void RestoreWindow();
        void SignIn(string displayName, string groupName, bool byUser);
        void SignOut();
        void StartBroadcastChat();
        void StartBroadcastChat(System.Collections.Generic.IEnumerable<Squiggle.Client.IBuddy> buddies);
        IChatWindow StartChat(Squiggle.Client.IBuddy buddy);
        void StartGroupChat(System.Collections.Generic.IEnumerable<Squiggle.Client.IBuddy> buddies);
        void ToggleMainWindow();
    }
}
