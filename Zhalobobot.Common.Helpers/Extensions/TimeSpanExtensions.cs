using System;
using Zhalobobot.Common.Models.Extensions;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class TimeSpanExtensions
{
    public static string ToHourAndMinute(this TimeSpan timeSpan)
        => $"{timeSpan.Hours.WithLeadingZeroIfLessThanTen()}:{(timeSpan.Minutes + 1).WithLeadingZeroIfLessThanTen()}";
}