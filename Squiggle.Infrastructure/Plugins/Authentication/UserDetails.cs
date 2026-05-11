namespace Squiggle.Plugins.Authentication
{
    public record UserDetails
    {
        public byte[]? Image { get; init; }
        public string? DisplayName { get; init; }
        public string? DisplayMessage { get; init; }
        public string? GroupName { get; init; }
        public string? Email { get; init; }
    }
}
