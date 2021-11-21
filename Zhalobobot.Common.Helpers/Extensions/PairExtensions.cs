using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class PairExtensions
    {
        public static (HourAndMinute Start, HourAndMinute End) ToHourAndMinute(this Pair pair)
            => pair switch
            {
                Pair.First => (new HourAndMinute(9, 0), new HourAndMinute(10, 30)),
                Pair.Second => (new HourAndMinute(10, 40), new HourAndMinute(12, 10)),
                Pair.Third => (new HourAndMinute(12, 50), new HourAndMinute(14, 20)),
                Pair.Fourth => (new HourAndMinute(14, 30), new HourAndMinute(16, 00)),
                Pair.Fifth => (new HourAndMinute(16, 10), new HourAndMinute(17, 40)),
                Pair.Sixth => (new HourAndMinute(17, 50), new HourAndMinute(19, 20)),
                Pair.Seventh => (new HourAndMinute(19, 30), new HourAndMinute(21, 00)),
                _ => throw new NotSupportedException(nameof(pair))
            };
    }
}