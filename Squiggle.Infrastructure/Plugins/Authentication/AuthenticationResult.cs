namespace Squiggle.Plugins.Authentication
{
    public record AuthenticationResult
    {
        public AuthenticationStatus Status { get; init; }
        public UserDetails UserDetails { get; init; }

        public AuthenticationResult(AuthenticationStatus status)
        {
            Status = status;
            UserDetails = new UserDetails();
        }
    }
}
