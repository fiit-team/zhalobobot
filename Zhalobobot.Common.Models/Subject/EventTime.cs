using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject
{
    public record EventTime(
        DayOfWeek DayOfWeek,
        Pair? Pair,
        HourAndMinute? StartTime,
        HourAndMinute? EndTime,
        DayAndMonth? StartDay,
        DayAndMonth? EndDay,
        WeekParity WeekParity);
}