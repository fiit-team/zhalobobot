using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnumsNET;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Bot.Schedule
{
    public class ScheduleMessageFormatter : IScheduleMessageFormatter
    {
        private const int TelegramMessageWidth = 30;

        private readonly EntitiesCache cache;
        private readonly bool isFirstYearWeekOdd;

        public ScheduleMessageFormatter(EntitiesCache cache, IConfiguration configuration)
        {
            this.cache = cache;
            isFirstYearWeekOdd = bool.Parse(configuration["IsFirstYearWeekOdd"]);
        }
        
        public string Format(long chatId, ScheduleDay scheduleDay, out DateOnly? whenDelete)
        {
            if (scheduleDay.IsCurrentWeek())
                return CurrentWeek(chatId, scheduleDay, out whenDelete);

            whenDelete = null;
            return NextWeek(chatId, scheduleDay);
        }

        private string CurrentWeek(long chatId, ScheduleDay day, out DateOnly? whenDelete)
        {
            var student = cache.Students.Get(chatId);

            var weekSchedule = cache.ScheduleItems
                .GetFor(student, DateHelper.CurrentWeekParity(isFirstYearWeekOdd))
                .ToArray();

            var weekByDay = weekSchedule
                .GroupBy(s => s.EventTime.DayOfWeek)
                .ToDictionary(s => (ScheduleDay)(int)s.Key, s => s.ToArray());

            var lastStudyWeekDay = weekSchedule.LastStudyWeekDay();

            string message;
            whenDelete = null;
            
            switch (day)
            {
                case >= ScheduleDay.Monday and <= ScheduleDay.Saturday:
                    message = FormatDay(weekByDay, day, day, true);
                    whenDelete = day.OneDayAfterCurrentWeekDayAndMonth();
                    break;
                case ScheduleDay.UntilWeekEnd:
                {
                    var currentDay = (int)DateHelper.EkbTime.DayOfWeek;

                    var days = Enum.GetValues<ScheduleDay>()
                        .Where(d => d >= (ScheduleDay)currentDay && d <= (ScheduleDay)(int)lastStudyWeekDay);

                    message = string.Join("\n", days.Select(d => FormatDay(weekByDay, d, d, true)).Where(s => s.Length > 0));
                    break;
                }
                case ScheduleDay.FullWeek:
                    var weekDays = Enum.GetValues<ScheduleDay>()
                        .Where(d => d >= ScheduleDay.Monday && d <= (ScheduleDay)(int)lastStudyWeekDay);
                    
                    message = string.Join("\n", weekDays.Select(d => FormatDay(weekByDay, d, day, true)).Where(s => s.Length > 0));
                    break;
                default:
                    throw new NotSupportedException();
            }
            
            return message;
        }

        private string NextWeek(long chatId, ScheduleDay day)
        {
            var student = cache.Students.Get(chatId);

            var weekSchedule = cache.ScheduleItems
                .GetFor(student, DateHelper.NextWeekParity(isFirstYearWeekOdd))
                .ToArray();
            
            var weekByDay = weekSchedule
                .GroupBy(s => s.EventTime.DayOfWeek)
                .ToDictionary(s => (ScheduleDay)(int)s.Key, s => s.ToArray());
            
            string message;

            switch (day)
            {
                case ScheduleDay.NextMonday:
                    message = FormatDay(weekByDay, ScheduleDay.Monday, ScheduleDay.Monday, false);
                    break;
                case ScheduleDay.NextWeek:
                    var weekDays = Enum.GetValues<ScheduleDay>()
                        .Where(d => d >= ScheduleDay.Monday && d <= (ScheduleDay)(int)weekSchedule.LastStudyWeekDay());

                    message = string.Join("\n", weekDays.Select(d => FormatDay(weekByDay, d, ScheduleDay.FullWeek, false)).Where(s => s.Length > 0));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return message;
        }
        
        private string FormatDay(IReadOnlyDictionary<ScheduleDay, ScheduleItem[]> scheduleByDay, ScheduleDay scheduleDay, ScheduleDay target, bool currentWeek)
        {
            if (!scheduleByDay.ContainsKey(scheduleDay))
                return "";

            var date = currentWeek ? scheduleDay.CurrentWeekDayAndMonth() : scheduleDay.NextWeekDayAndMonth();
            var schedule = scheduleByDay[scheduleDay]
                .EmptyWhenHolidays(date, cache.Holidays.All.FromRecord().ToHashSet());

            if (schedule.Length == 0)
                return "";

            var builder = new StringBuilder();
            var dayOfWeek = scheduleDay.AsString(EnumFormat.Description);
            builder.Append($"{dayOfWeek} {date.ToDayAndMonth()}".PutInCenterOf(' ', TelegramMessageWidth) + "\n");

            var hourAndMinute = DateHelper.EkbTime.ToTimeOnly();

            var actualSchedule = schedule
                .GroupBy(s => s.Subject.Name)
                .SelectMany(group => GetActualSchedule(group, date));

            var orderedItems = OrderSchedule(actualSchedule);

            var firstItem = orderedItems.First();

            builder.Append(SingleDayScheduleRequested() ? ForDay(firstItem) : ForWeek(firstItem));

            foreach (var item in orderedItems.Skip(1))
            {
                var (_, previousEnd) = GetSubjectDuration(firstItem);
                var (nextStart, _) = GetSubjectDuration(item);

                if (previousEnd < hourAndMinute && hourAndMinute < nextStart && item.EventTime.DayOfWeek == DateHelper.EkbTime.DayOfWeek)
                {
                    var diff = nextStart - hourAndMinute;
                    builder.Append($"[{hourAndMinute}, до пары {(diff.Hours == 0 ? $"{diff.Minutes}мин" : diff.ToHourAndMinute())}]".PutInCenterOf('-', TelegramMessageWidth) + "\n");
                }
                else if (SingleDayScheduleRequested())
                    builder.Append($"{new string(' ', TelegramMessageWidth)}\n");
                
                builder.Append(SingleDayScheduleRequested() ? ForDay(item) : ForWeek(item));

                firstItem = item;
            }
            
            return builder.ToString();

            string ForWeek(ScheduleItem item) =>$"{Crop(FormatItemForPeriod(item))}\n";

            string ForDay(ScheduleItem item)
            {
                var (start, end) = GetSubjectDuration(item);

                var (firstLine, secondLine) = FormatItemForDay(item);

                if (start <= hourAndMinute && hourAndMinute <= end &&
                    item.EventTime.DayOfWeek == DateHelper.EkbTime.DayOfWeek)
                {
                    var diff = end - hourAndMinute;
                    var timeBetweenStudy = $"[{hourAndMinute}, до конца {(diff.Hours == 0 ? $"{diff.Minutes}мин" : diff.ToHourAndMinute())}]".PutInCenterOf('-', TelegramMessageWidth);
                    return $"{Crop(firstLine)}\n{timeBetweenStudy}\n{Crop(secondLine)}\n";
                }

                return $"{Crop(firstLine)}\n{Crop(secondLine)}\n";
            }

            bool SingleDayScheduleRequested() =>
                target is >= ScheduleDay.Monday and <= ScheduleDay.Saturday;

            string Crop(string str)
            {
                var maxLength = Math.Min(TelegramMessageWidth, str.Length);
                return $"{str[..maxLength]}{(str.Length > TelegramMessageWidth ? "…" : "")}";
            }

            IEnumerable<ScheduleItem> GetActualSchedule(IEnumerable<ScheduleItem> items, DateOnly day)
            {
                var hourAndMinuteToScheduleItem = items
                    .GroupBy(i => GetSubjectDuration(i).Start)
                    .ToDictionary(i => i.Key, i => i.ToArray());

                foreach (var (_, scheduleItems) in hourAndMinuteToScheduleItem)
                {
                    switch (scheduleItems.Length)
                    {
                        case 1:
                            yield return scheduleItems.First();
                            break;
                        case > 1:
                        {
                            var bothStartAndEndDay = scheduleItems
                                .Where(k =>
                                    k.EventTime.StartDay != null && k.EventTime.StartDay <= day &&
                                    k.EventTime.EndDay != null && k.EventTime.EndDay >= day)
                                .ToArray();
                            if (bothStartAndEndDay.Length == 0)
                            {
                                var noStartAndEndDay = scheduleItems
                                    .Where(o => o.EventTime.StartDay == null && o.EventTime.EndDay == null)
                                    .ToArray();
                                if (noStartAndEndDay.Length == 1)
                                    yield return noStartAndEndDay.First();
                                else
                                {
                                    var startOrEndDay = OrderSchedule(scheduleItems
                                        .Where(o => o.EventTime.StartDay == null || o.EventTime.EndDay == null)
                                        .ToArray());
                                    yield return startOrEndDay.First();
                                }
                            }
                            else
                                yield return bothStartAndEndDay
                                    .OrderBy(s => date.ToDateTime(TimeOnly.MinValue).Subtract(s.EventTime.StartDay!.Value.ToDateTime(TimeOnly.MinValue)))
                                    .First();

                            break;
                        }
                    }
                }
            }

            static ScheduleItem[] OrderSchedule(IEnumerable<ScheduleItem> schedule)
            {
                return schedule
                    .OrderBy(i => GetSubjectDuration(i).Start.Hour)
                    .ThenBy(i => GetSubjectDuration(i).Start.Minute)
                    .ThenBy(i => GetSubjectDuration(i).End.Hour)
                    .ThenBy(i => GetSubjectDuration(i).End.Minute)
                    .ToArray();
            }
        }

        private static (string FirstLine, string SecondLine) FormatItemForDay(ScheduleItem item)
        {
            var (start, end) = GetSubjectDuration(item);

            return ($"{start} {item.Subject.Name}", $"{end} {item.Cabinet}");
        }

        private static string FormatItemForPeriod(ScheduleItem item)
        {
            var (start, _) = GetSubjectDuration(item);

            return $"{start} {item.Subject.Name}";
        }

        private static (TimeOnly Start, TimeOnly End) GetSubjectDuration(ScheduleItem item)
        {
            var start = item.EventTime.StartTime 
               ?? item.EventTime.Pair?.ToTimeOnly().Start 
               ?? throw new Exception();
            
            var end = item.EventTime.EndTime 
               ?? item.EventTime.Pair?.ToTimeOnly().End 
               ?? throw new Exception();

            return (start, end);
        }
    }
}