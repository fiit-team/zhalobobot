using System;
using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class ScheduleItemExtensions
    {
        public static DayOfWeek LastStudyWeekDay(this IEnumerable<ScheduleItem> items) =>
            items.Select(s => s.EventTime.DayOfWeek)
                .Distinct()
                .OrderByDescending(d => d)
                .First();

        public static ScheduleItem[] EmptyWhenHolidays(this IEnumerable<ScheduleItem> items, DayAndMonth date, HashSet<DayAndMonth> holidays)
        {
            return holidays.Contains(date) 
                ? Array.Empty<ScheduleItem>() 
                : items.ToArray();
        }
    }
}