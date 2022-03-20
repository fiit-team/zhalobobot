using System;
using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Common.Helpers.Extensions;

public static class ScheduleItemExtensions
{
    public static IEnumerable<ScheduleItem> ActualSchedule(
        this IEnumerable<ScheduleItem> items,
        HashSet<DateOnly> holidays,
        DateTime date,
        IEnumerable<DayWithoutPairs> daysWithoutPair)
    {
        var day = date.ToDateOnly();

        var startHourAndMinuteToScheduleItems = items
            .EmptyWhenHolidays(date.ToDateOnly(), holidays)
            .Where(FilterActualScheduleItem)
            .GroupBy(i => GetSubjectDuration(i).Start);
        
        foreach (var scheduleItemsStartsAndEndsInSameTime in startHourAndMinuteToScheduleItems)
        {
            if (scheduleItemsStartsAndEndsInSameTime.Count() == 1)
                yield return scheduleItemsStartsAndEndsInSameTime.First();
            else
            {
                foreach (var subjects in scheduleItemsStartsAndEndsInSameTime.GroupBy(i => (i.Subject.Name, i.Subject.Course, i.Group, i.Subgroup)))
                {
                    if (subjects.Count() == 1)
                        yield return subjects.First();
                    else
                        yield return SelectItemFromItemsWithEqualName(subjects);
                }
            }
        }

        bool FilterActualScheduleItem(ScheduleItem item)
        {
            if (item.EventTime.DayOfWeek != date.DayOfWeek)
                return false;
            
            if (item.GetSubjectDuration().End != date.ToHourAndMinute())
                return false;

            if (item.EventTime.StartDay.HasValue && item.EventTime.StartDay.Value > date.ToDateOnly())
                return false;

            if (item.EventTime.EndDay.HasValue && item.EventTime.EndDay.Value < date.ToDateOnly())
                return false;

            if (item.ShouldSkipItem(daysWithoutPair, date))
                return false;

            return true;
        }

        ScheduleItem SelectItemFromItemsWithEqualName(IGrouping<(string, Course, Group, Subgroup), ScheduleItem> subjects)
        {
            var bothStartAndEndDay = subjects
                .Where(i => i.EventTime.StartDay.HasValue && i.EventTime.EndDay.HasValue)
                .ToArray();
            
            if (bothStartAndEndDay.Length == 0)
            {
                var startOrEndDay = subjects
                    .Where(i => i.EventTime.StartDay.HasValue || i.EventTime.EndDay.HasValue)
                    .ToArray();
                if (startOrEndDay.Length == 0)
                {
                    // note (d.stukov): because items have equal names, startTime, endTime, and no startDay or endDay
                    return subjects.First();
                }
                
                var haveStartDay = startOrEndDay
                    .Where(i => i.EventTime.StartDay.HasValue)
                    .ToArray();
                if (haveStartDay.Length == 0)
                {
                    return startOrEndDay
                        .OrderBy(i => i.EventTime.EndDay!.Value.ToDateTime(TimeOnly.MinValue).Subtract(day.ToDateTime(TimeOnly.MinValue)))
                        .First();
                }
                
                return haveStartDay
                    .OrderBy(i => day.ToDateTime(TimeOnly.MinValue).Subtract(i.EventTime.StartDay!.Value.ToDateTime(TimeOnly.MinValue)))
                    .First();
            }

            return bothStartAndEndDay
                .OrderBy(i => day.ToDateTime(TimeOnly.MinValue).Subtract(i.EventTime.StartDay!.Value.ToDateTime(TimeOnly.MinValue)))
                .First();
        }
    }
    
    public static IEnumerable<ScheduleItem> OrderSchedule(this IEnumerable<ScheduleItem> schedule)
    {
        return schedule
            .OrderBy(i => GetSubjectDuration(i).Start.TotalMinutes())
            .ThenBy(i => GetSubjectDuration(i).End.TotalMinutes());
    }
    
    public static (HourAndMinute Start, HourAndMinute End) GetSubjectDuration(this ScheduleItem item)
    {
        var start = item.EventTime.StartTime?.ToHourAndMinute()
            ?? item.EventTime.Pair?.ToHourAndMinute().Start
            ?? throw new Exception();

        var end = item.EventTime.EndTime?.ToHourAndMinute()
            ?? item.EventTime.Pair?.ToHourAndMinute().End
            ?? throw new Exception();

        return (start, end);
    }

    public static DayOfWeek LastStudyWeekDay(this IEnumerable<ScheduleItem> items) =>
        items.Select(i => i.EventTime.DayOfWeek)
            .Distinct()
            .Max();
    
    public static IEnumerable<ScheduleItem> For(this IEnumerable<ScheduleItem> items, Student student, WeekParity weekParity) 
        => items
            .Where(i => FilterByWeekParityCourseGroupSubgroup(i, student, weekParity))
            .Distinct();

    private static bool FilterByWeekParityCourseGroupSubgroup(ScheduleItem item, Student student, WeekParity weekParity)
    {
        if (item.EventTime.WeekParity != WeekParity.Both && item.EventTime.WeekParity != weekParity)
            return false;
        
        return item.Subject.Course == student.Course && item.Group == student.Group && item.Subgroup == student.Subgroup;
    }

    private static IEnumerable<ScheduleItem> EmptyWhenHolidays(this IEnumerable<ScheduleItem> items, DateOnly date, IReadOnlySet<DateOnly> holidays)
        => holidays.Contains(date) 
            ? Enumerable.Empty<ScheduleItem>() 
            : items;

    private static bool ShouldSkipItem(this ScheduleItem item, IEnumerable<DayWithoutPairs> daysWithoutPairs, DateTime date)
    {
        return daysWithoutPairs
            .Where(MatchWithScheduleItem)
            .Any();
        
        bool MatchWithScheduleItem(DayWithoutPairs dayWithoutPairs)
        {
            if (dayWithoutPairs.Date == date.ToDateOnly()
                && dayWithoutPairs.SubjectName == item.Subject.Name
                && dayWithoutPairs.Course == item.Subject.Course
                && dayWithoutPairs.Group == item.Group
                && dayWithoutPairs.Subgroup == item.Subgroup)
            {
                if (dayWithoutPairs.EndTime.HasValue)
                    return dayWithoutPairs.EndTime.Value.ToHourAndMinute() == item.GetSubjectDuration().End;

                return true;
            }
        
            return false;
        }
    }
}