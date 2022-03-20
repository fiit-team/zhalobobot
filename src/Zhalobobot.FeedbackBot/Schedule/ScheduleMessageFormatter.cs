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

        public static string Format(ScheduleItem[] schedule, ScheduleDay scheduleDay, out DateOnly? whenDelete)
        {
            var dayToSchedule = schedule
                .GroupBy(s => s.EventTime.DayOfWeek)
                .ToDictionary(s => (ScheduleDay)(int)s.Key, s => s.ToArray());

            var lastStudyWeekDay = schedule.LastStudyWeekDay();
            
            if (scheduleDay.IsCurrentWeek())
                return CurrentWeek(dayToSchedule, lastStudyWeekDay, scheduleDay, DateHelper.EkbTime, out whenDelete);

            whenDelete = null;
            return NextWeek(dayToSchedule, lastStudyWeekDay, scheduleDay, DateHelper.EkbTime);
        }

        private static string CurrentWeek(IReadOnlyDictionary<ScheduleDay, ScheduleItem[]> dayToSchedule, DayOfWeek lastStudyWeekDay, ScheduleDay day, DateTime dateTime, out DateOnly? whenDelete)
        {
            whenDelete = null;
            
            switch (day)
            {
                case >= ScheduleDay.Monday and <= ScheduleDay.Sunday:
                    whenDelete = day.OneDayAfterCurrentWeekDayAndMonth();
                    return FormatMessage(dayToSchedule, day, day, true, dateTime);
                case ScheduleDay.UntilWeekEnd:
                {
                    var currentDay = (int)dateTime.DayOfWeek;

                    var days = Enum.GetValues<ScheduleDay>()
                        .Where(d => d >= (ScheduleDay)currentDay && d <= (ScheduleDay)(int)lastStudyWeekDay);

                    return string.Join("\n", days.Select(d => FormatMessage(dayToSchedule, d, d, true, dateTime)).Where(s => s.Length > 0));
                }
                case ScheduleDay.FullWeek:
                    var weekDays = Enum.GetValues<ScheduleDay>()
                        .Where(d => d >= ScheduleDay.Monday && d <= (ScheduleDay)(int)lastStudyWeekDay);
                    
                    return string.Join("\n", weekDays.Select(d => FormatMessage(dayToSchedule, d, day, true, dateTime)).Where(s => s.Length > 0));
                default:
                    throw new NotSupportedException();
            }
        }

        private static string NextWeek(IReadOnlyDictionary<ScheduleDay, ScheduleItem[]> dayToSchedule, DayOfWeek lastStudyWeekDay, ScheduleDay day, DateTime dateTime)
        {
            switch (day)
            {
                case ScheduleDay.NextMonday:
                    return FormatMessage(dayToSchedule, ScheduleDay.Monday, ScheduleDay.Monday, false, dateTime);
                case ScheduleDay.NextWeek:
                    var weekDays = Enum.GetValues<ScheduleDay>()
                        .Where(d => d >= ScheduleDay.Monday && d <= (ScheduleDay)(int)lastStudyWeekDay);

                    return string.Join("\n", weekDays.Select(d => FormatMessage(dayToSchedule, d, ScheduleDay.FullWeek, false, dateTime)).Where(s => s.Length > 0));
                default:
                    throw new NotSupportedException();
            }
        }
        
        private static string FormatMessage(
            IReadOnlyDictionary<ScheduleDay, ScheduleItem[]> dayToSchedule, 
            ScheduleDay scheduleDay, 
            ScheduleDay target, 
            bool currentWeek,
            DateTime dateTime)
        {
            if (!dayToSchedule.ContainsKey(scheduleDay))
                return "";

            var date = currentWeek ? scheduleDay.CurrentWeekDayAndMonth() : scheduleDay.NextWeekDayAndMonth();
            
            var schedule = dayToSchedule[scheduleDay];

            if (schedule.Length == 0)
                return "";

            var builder = new StringBuilder();
            var dayOfWeek = scheduleDay.AsString(EnumFormat.Description);
            builder.Append($"{dayOfWeek} {date.ToDayAndMonth()}".PutInCenterOf(' ', TelegramMessageWidth) + "\n");

            var currentTime = dateTime.ToHourAndMinute();

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

                if (previousEnd < currentTime && currentTime < nextStart && item.EventTime.DayOfWeek == dateTime.DayOfWeek)
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

                if (start <= currentTime && currentTime <= end && item.EventTime.DayOfWeek == dateTime.DayOfWeek)
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