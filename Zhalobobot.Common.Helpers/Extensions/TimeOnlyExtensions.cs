using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class TimeOnlyExtensions
{
    public static HourAndMinute ToHourAndMinute(this TimeOnly timeOnly) => new(timeOnly.Hour, timeOnly.Minute);
}