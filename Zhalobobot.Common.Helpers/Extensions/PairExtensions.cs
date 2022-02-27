using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class PairExtensions
    {
        public static (TimeOnly Start, TimeOnly End) ToTimeOnly(this Pair pair)
            => pair switch
            {
                Pair.First => (new TimeOnly(9, 0), new TimeOnly(10, 30)),
                Pair.Second => (new TimeOnly(10, 40), new TimeOnly(12, 10)),
                Pair.Third => (new TimeOnly(12, 50), new TimeOnly(14, 20)),
                Pair.Fourth => (new TimeOnly(14, 30), new TimeOnly(16, 00)),
                Pair.Fifth => (new TimeOnly(16, 10), new TimeOnly(17, 40)),
                Pair.Sixth => (new TimeOnly(17, 50), new TimeOnly(19, 20)),
                Pair.Seventh => (new TimeOnly(19, 30), new TimeOnly(21, 00)),
                _ => throw new NotSupportedException(nameof(pair))
            };
    }
}