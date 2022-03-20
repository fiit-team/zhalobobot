using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class PairExtensions
{
    public static (HourAndMinute Start, HourAndMinute End) ToHourAndMinute(this Pair pair)
        => pair switch
        {
            Pair.First => (new (9, 0), new (10, 30)),
            Pair.Second => (new (10, 40), new (12, 10)),
            Pair.Third => (new (12, 50), new (14, 20)),
            Pair.Fourth => (new (14, 30), new (16, 00)),
            Pair.Fifth => (new (16, 10), new (17, 40)),
            Pair.Sixth => (new (17, 50), new (19, 20)),
            Pair.Seventh => (new (19, 30), new (21, 00)),
            _ => throw new NotSupportedException(nameof(pair))
        };
}