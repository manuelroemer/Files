namespace Files.Utilities
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    internal static class DictionaryExtensions
    {

        [return: MaybeNull]
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                return default!;
            }
        }

    }

}
