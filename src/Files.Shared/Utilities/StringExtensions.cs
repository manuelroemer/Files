namespace Files.Utilities
{
    internal static class StringExtensions
    {
        public static string? ToNullIfEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? null : str;
        }

        public static bool Contains(this string str, char[] characters)
        {
            return str.IndexOfAny(characters) >= 0;
        }
    }
}
