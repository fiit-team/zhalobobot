using System;
using Zhalobobot.Common.Models.Extensions;

namespace Zhalobobot.Bot.Models;

public record HourAndMinute(int Hour, int Minute) : IComparable<HourAndMinute>
{
    public int CompareTo(HourAndMinute? other)
    {
        if (ReferenceEquals(this, other))
            return 0;
        if (ReferenceEquals(null, other))
            return 1;

        var hourComparison = Hour.CompareTo(other.Hour);

        return hourComparison != 0 
            ? hourComparison 
            : Minute.CompareTo(other.Minute);        
    }

    public static bool operator >(HourAndMinute left, HourAndMinute right) => left.CompareTo(right) > 0;

    public static bool operator <(HourAndMinute left, HourAndMinute right) => left.CompareTo(right) < 0;

    public static bool operator >=(HourAndMinute left, HourAndMinute right) => left.CompareTo(right) >= 0;

    public static bool operator <=(HourAndMinute left, HourAndMinute right) => left.CompareTo(right) <= 0;

    public static HourAndMinute operator -(HourAndMinute left, HourAndMinute right)
    {
        var totalMinutes = left.Hour * 60 + left.Minute - (right.Hour * 60 + right.Minute);

        return new HourAndMinute(totalMinutes / 60, totalMinutes % 60);
    }

    public override string ToString()
        => $"{Hour.WithLeadingZeroIfLessThanTen()}:{Minute.WithLeadingZeroIfLessThanTen()}";
}