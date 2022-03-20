using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class HourAndMinuteExtensions
{
    public static int TotalMinutes(this HourAndMinute hourAndMinute)
        => hourAndMinute.Hour * 60 + hourAndMinute.Minute;
}