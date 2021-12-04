using System.Collections.Generic;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue? Find<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue>? dictionary, TKey key)
            where TKey : notnull
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out var value))
                return default;
            return value;
        }
    }
}