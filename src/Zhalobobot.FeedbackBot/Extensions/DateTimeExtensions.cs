using System;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Extensions;

public static class DateTimeExtensions
{
    public static HourAndMinute ToHourAndMinute(this DateTime dateTime)
        => new (dateTime.Hour, dateTime.Minute);
}