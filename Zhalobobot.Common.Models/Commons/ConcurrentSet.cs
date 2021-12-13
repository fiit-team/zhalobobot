using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Zhalobobot.Common.Models.Commons
{
    public class ConcurrentSet<T> : ICollection<T>
        where T : notnull
    {
        public ConcurrentSet()
        {
            dictionary = new ConcurrentDictionary<T, byte>();
        }

        public ConcurrentSet(IEqualityComparer<T> comparer)
        {
            dictionary = new ConcurrentDictionary<T, byte>(comparer);
        }

        public ConcurrentSet(Int32 concurrencyLevel, Int32 capacity)
        {
            dictionary = new ConcurrentDictionary<T, byte>(concurrencyLevel, capacity);
        }

        public ConcurrentSet(IEnumerable<T> items)
            : this()
        {
            foreach (var item in items)
                Add(item);
        }

        public bool Add(T item) => dictionary.TryAdd(item, 0);

        public bool Contains(T item) => dictionary.ContainsKey(item);

        public void CopyTo(T[] array, int arrayIndex) => dictionary.Keys.CopyTo(array, arrayIndex);

        public bool Remove(T item) => dictionary.TryRemove(item, out var value);

        void ICollection<T>.Add(T item) => Add(item);

        public void Clear() => dictionary.Clear();

        public IEnumerator<T> GetEnumerator() => dictionary.Select(pair => pair.Key).GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        private readonly ConcurrentDictionary<T, byte> dictionary;
    }
}