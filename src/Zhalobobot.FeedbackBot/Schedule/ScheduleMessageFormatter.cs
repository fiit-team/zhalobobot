using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnumsNET;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Bot.Schedule
{
    public static class ScheduleMessageFormatter
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
                case >= ScheduleDay.Monday and <= ScheduleDay.Sunday:
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

            var currentTime = DateHelper.EkbTime.ToHourAndMinute();

            var actualSchedule = schedule
                .GroupBy(s => s.Subject.Name)
                .SelectMany(group => GetActualSchedule(group, date));

            var orderedItems = OrderSchedule(actualSchedule);

            var firstItem = orderedItems.First();

            builder.Append(SingleDayScheduleRequested() ? ForDay(firstItem) : ForWeek(firstItem));

            foreach (var item in orderedItems.Skip(1))
            {
                var (_, previousEnd) = firstItem.GetSubjectDuration();
                var (nextStart, _) = item.GetSubjectDuration();

                if (previousEnd < currentTime && currentTime < nextStart && item.EventTime.DayOfWeek == DateHelper.EkbTime.DayOfWeek)
                {
                    var diff = nextStart - currentTime;
                    builder.Append($"[{currentTime}, до пары {(diff.Hour == 0 ? $"{diff.Minute}мин" : diff)}]".PutInCenterOf('-', TelegramMessageWidth) + "\n");
                }
                else if (SingleDayScheduleRequested())
                    builder.Append('\n');
                
                builder.Append(SingleDayScheduleRequested() ? ForDay(item) : ForWeek(item));

                firstItem = item;
            }
            
            return builder.ToString();

            string ForWeek(ScheduleItem item) =>$"{Crop(FormatItemForPeriod(item))}\n";

            string ForDay(ScheduleItem item)
            {
                var (start, end) = item.GetSubjectDuration();

                var (firstLine, secondLine) = FormatItemForDay(item);

                if (start <= currentTime && currentTime <= end && item.EventTime.DayOfWeek == DateHelper.EkbTime.DayOfWeek)
                {
                    var diff = end - currentTime;
                    var timeBetweenStudy = $"[{currentTime}, до конца {(diff.Hour == 0 ? $"{diff.Minute}мин" : diff)}]".PutInCenterOf('-', TelegramMessageWidth);
                    return $"{Crop(firstLine)}\n{timeBetweenStudy}\n{Crop(secondLine)}\n";
                }

                return $"{Crop(firstLine)}\n{Crop(secondLine)}\n";
            }

            bool SingleDayScheduleRequested() =>
                target is >= ScheduleDay.Monday and <= ScheduleDay.Sunday;

            static string Crop(string str)
            {
                var maxLength = Math.Min(TelegramMessageWidth, str.Length);
                return $"{str[..maxLength]}{(str.Length > TelegramMessageWidth ? "…" : "")}";
            }
        }

        private static (string FirstLine, string SecondLine) FormatItemForDay(ScheduleItem item)
        {
            var (start, end) = item.GetSubjectDuration();

            return ($"{start} {item.Subject.Name}", $"{end} {item.Cabinet}");
        }

        private static string FormatItemForPeriod(ScheduleItem item)
        {
            var (start, _) = item.GetSubjectDuration();

            return $"{start} {item.Subject.Name}";
        }
    }
}