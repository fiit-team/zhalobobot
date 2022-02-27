using System;
using Zhalobobot.Common.Models.Extensions;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class DateOnlyExtensions
{
    public static string ToDayAndMonth(this DateOnly dateOnly)
        => $"{dateOnly.Day.WithLeadingZeroIfLessThanTen()}.{dateOnly.Month.WithLeadingZeroIfLessThanTen()}";
}