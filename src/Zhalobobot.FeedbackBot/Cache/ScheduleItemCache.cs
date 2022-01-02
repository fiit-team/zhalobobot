using System;
using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Bot.Cache
{
    public class ScheduleItemCache : EntityCacheBase<ScheduleItem>
    {
        private readonly Dictionary<WeekParity, List<ScheduleItem>> weekParityIndex = new();
        private readonly Dictionary<(Course, WeekParity), List<ScheduleItem>> courseAndWeekParityIndex = new();
        private readonly Dictionary<(Course, WeekParity), List<ScheduleItem>> courseAndWeekParityIndexIncludingBoth = new();

        public ScheduleItemCache(ScheduleItem[] scheduleItems)
            : base(scheduleItems)
        {
            foreach (var item in scheduleItems)
            {
                weekParityIndex.AddOrUpdate(item.EventTime.WeekParity, new List<ScheduleItem> {item}, items => items.Add(item));
                courseAndWeekParityIndex.AddOrUpdate((item.Subject.Course, item.EventTime.WeekParity), new List<ScheduleItem> {item}, items => items.Add(item));

                if (item.EventTime.WeekParity == WeekParity.Both)
                {
                    courseAndWeekParityIndexIncludingBoth.AddOrUpdate((item.Subject.Course, WeekParity.Odd), new List<ScheduleItem> {item}, items => items.Add(item));
                    courseAndWeekParityIndexIncludingBoth.AddOrUpdate((item.Subject.Course, WeekParity.Even), new List<ScheduleItem> {item}, items => items.Add(item));
                }
                else
                {
                    courseAndWeekParityIndexIncludingBoth.AddOrUpdate((item.Subject.Course, item.EventTime.WeekParity), new List<ScheduleItem> {item}, items => items.Add(item));
                }
            }
        }

        public List<ScheduleItem> GetOnly(WeekParity weekParity) => weekParityIndex.GetOrCreate(weekParity, _ => new List<ScheduleItem>());
        public List<ScheduleItem> GetOnly(Course course, WeekParity weekParity) => courseAndWeekParityIndex.GetOrCreate((course, weekParity), _ => new List<ScheduleItem>());
        public List<ScheduleItem> GetFullFor(Course course, WeekParity weekParity) => courseAndWeekParityIndexIncludingBoth.GetOrCreate((course, weekParity), _ => new List<ScheduleItem>());

        public IEnumerable<ScheduleItem> GetByDayOfWeekAndEndsAtHourAndMinute(DayOfWeek dayOfWeek, HourAndMinute hourAndMinute)
        {
            return All.Where(s => s.EventTime.DayOfWeek == dayOfWeek && EndTimeMatches(s));

            bool EndTimeMatches(ScheduleItem item)
                => item.EventTime.Pair.HasValue && item.EventTime.Pair.Value.ToHourAndMinute().End == hourAndMinute
                   || item.EventTime.EndTime != null && item.EventTime.EndTime == hourAndMinute;
        } 
    }
}