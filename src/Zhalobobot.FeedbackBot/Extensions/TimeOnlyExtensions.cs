using System;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Extensions;

public static class TimeOnlyExtensions
{
    public static HourAndMinute ToHourAndMinute(this TimeOnly timeOnly) => new(timeOnly.Hour, timeOnly.Minute);
}