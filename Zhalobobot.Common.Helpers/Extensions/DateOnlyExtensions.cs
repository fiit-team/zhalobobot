using System;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class DateOnlyExtensions
{
    public static string ToDayAndMonth(this DateOnly dateOnly)
        => $"{dateOnly.Day:00}.{dateOnly.Month:00}";
}