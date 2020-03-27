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
            foreach (var actualChar in str)
            {
                foreach (var searchedChar in characters)
                {
                    if (actualChar == searchedChar)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
