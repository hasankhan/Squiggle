namespace Squiggle.Plugins
{
    public record SignInOptions
    {
        public string? Username { get; init; }
        public string? Password { get; init; }
        public string? Domain { get; init; }
        public string? DisplayName { get; init; }
        public string? GroupName { get; init; }
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
