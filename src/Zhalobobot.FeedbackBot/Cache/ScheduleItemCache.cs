using System;
using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Bot.Extensions;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Student;

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
        public IEnumerable<ScheduleItem> GetFor(Student student, WeekParity weekParity) =>
            courseAndWeekParityIndexIncludingBoth
                .GetOrCreate((student.Course, weekParity), _ => new List<ScheduleItem>())
                .Where(s => s.Group == student.Group && (!s.Subgroup.HasValue || s.Subgroup.Value == student.Subgroup))
                .Distinct();

        public IEnumerable<ScheduleItem> GetByDayOfWeekAndEndsAtTime(DayOfWeek dayOfWeek, HourAndMinute currentTime)
        {
            return All.Where(s => s.EventTime.DayOfWeek == dayOfWeek && EndTimeMatches(s));

            bool EndTimeMatches(ScheduleItem item)
            {
                HourAndMinute? end = null; 
                if (item.EventTime.Pair.HasValue)
                    end = item.EventTime.Pair.Value.ToHourAndMinute().End;
                else if (item.EventTime.EndTime.HasValue)
                    end = item.EventTime.EndTime.Value.ToHourAndMinute();

                return end != null && end == currentTime;
            }
        } 
    }
}