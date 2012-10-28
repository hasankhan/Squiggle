using System;

namespace Squiggle.Plugins
{

    public class SignInOptions
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public string Email { get; set; }
    }

    public interface IMainWindow
    {
        void BlinkTrayIcon();
        void InitializeComponent();
        void Quit();
        void RestoreWindow();
        void SignIn(SignInOptions options);
        void SignOut();
        void StartBroadcastChat();
        void StartBroadcastChat(System.Collections.Generic.IEnumerable<Squiggle.Client.IBuddy> buddies);
        IChatWindow StartChat(Squiggle.Client.IBuddy buddy);
        void StartGroupChat(System.Collections.Generic.IEnumerable<Squiggle.Client.IBuddy> buddies);
        void ToggleMainWindow();
    }
}
