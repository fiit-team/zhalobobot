using System;
using System.Collections.Generic;
using System.Linq;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            return items.Where(i => !predicate(i));
        }
    }
}