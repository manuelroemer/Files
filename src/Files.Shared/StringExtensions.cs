namespace Files.Shared
{
    internal static class StringExtensions
    {
        internal static string? ToNullIfEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? null : str;
        }

        internal static bool Contains(this string str, char[] characters)
        {
            return str.IndexOfAny(characters) >= 0;
        }
    }
}
