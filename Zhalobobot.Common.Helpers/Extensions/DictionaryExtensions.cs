using System;
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

        public static TValue Get<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
        {
            return dictionary.Find(key) ?? throw new KeyNotFoundException();
        }
        
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, Action<TValue> update)
        {
            if (dictionary.TryGetValue(key, out var currentValue))
                update(currentValue);
            else
                dictionary[key] = value;
        }
        
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFunc)
        {
            if (!dict.TryGetValue(key, out var value))
                return dict[key] = valueFunc(key);
            return value;
        }
    }
}