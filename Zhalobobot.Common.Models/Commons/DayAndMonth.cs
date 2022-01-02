using System;
using Zhalobobot.Common.Models.Extensions;

namespace Zhalobobot.Common.Models.Commons
{
    public record DayAndMonth(int Day, Month Month) : IComparable<DayAndMonth>
    {
        public int CompareTo(DayAndMonth? other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (ReferenceEquals(null, other))
                return 1;
            
            var monthComparison = Month.CompareTo(other.Month);
            
            return monthComparison != 0 
                ? monthComparison 
                : Day.CompareTo(other.Day);
        }

        public static bool operator >(DayAndMonth left, DayAndMonth right) => left.CompareTo(right) > 0;

        public static bool operator <(DayAndMonth left, DayAndMonth right) => left.CompareTo(right) < 0;

        public static bool operator >=(DayAndMonth left, DayAndMonth right) => left.CompareTo(right) >= 0;

        public static bool operator <=(DayAndMonth left, DayAndMonth right) => left.CompareTo(right) <= 0;

        public override string ToString() => $"{Day.WithLeadingZeroIfLessThanTen()}.{((int)Month).WithLeadingZeroIfLessThanTen()}";
    }
}