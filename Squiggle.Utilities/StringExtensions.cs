
namespace System
{
    public static class StringExtensions
    {
        public static string EmptyIfNull(this string text)
        {
            return text ?? String.Empty;
        }

        public static string NullIfEmpty(this string text)
        {
            return String.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
