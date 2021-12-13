using System;
using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class ScheduleItemExtensions
    {
        public static ScheduleItem[] FilterFor(this IEnumerable<ScheduleItem> items, Student student) =>
            items.Where(s => s.Group == student.Group && (!s.Subgroup.HasValue || s.Subgroup.Value == student.Subgroup))
                .Distinct()
                .ToArray();
        
        public static DayOfWeek LastStudyWeekDay(this IEnumerable<ScheduleItem> items) =>
            items.Select(s => s.EventTime.DayOfWeek)
                .Distinct()
                .OrderByDescending(d => d)
                .First();
    }
}