using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject
{
    public record EventTime(
        DayOfWeek DayOfWeek,
        Pair? Pair,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DateOnly? StartDay,
        DateOnly? EndDay,
        WeekParity WeekParity);
}