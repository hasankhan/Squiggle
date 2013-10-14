using System;

namespace Squiggle.Plugins
{

    public class SignInOptions
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
    }

    public interface IMainWindow: IWindow
    {
        void BlinkTrayIcon();
        void Quit();
        void SignIn(SignInOptions options);
        void SignOut();
        void StartBroadcastChat();
        void StartBroadcastChat(System.Collections.Generic.IEnumerable<Squiggle.Client.IBuddy> buddies);
        IChatWindow StartChat(Squiggle.Client.IBuddy buddy);
        void StartGroupChat(System.Collections.Generic.IEnumerable<Squiggle.Client.IBuddy> buddies);
        void ToggleMainWindow();
    }
}
