using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class DateTimeExtensions
{
    public static HourAndMinute ToHourAndMinute(this DateTime dateTime)
        => new (dateTime.Hour, dateTime.Minute);
}