using System;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Bot.Helpers
{
    public static class ScheduleDayExtensions
    {
        public static bool IsCurrentWeek(this ScheduleDay day) =>
            day is not (ScheduleDay.NextMonday or ScheduleDay.NextWeek);

        public static DateOnly CurrentWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.MondayDate.AddDays(date - ScheduleDay.Monday).ToDateOnly();
        }

        public static DateOnly OneDayAfterCurrentWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.MondayDate.AddDays(1 + date - ScheduleDay.Monday).ToDateOnly();
        }
        
        public static DateOnly NextWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.NextMondayDate.AddDays(date - ScheduleDay.Monday).ToDateOnly();
        }
        
        public static DateOnly OneDayAfterNextWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.NextMondayDate.AddDays(1 + date - ScheduleDay.Monday).ToDateOnly();
        }
    }
}