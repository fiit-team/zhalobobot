using System;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Bot.Helpers
{
    public static class ScheduleDayExtensions
    {
        public static DayAndMonth CurrentWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.MondayDate.AddDays(date - ScheduleDay.Monday).ToDayAndMonth();
        }

        public static DayAndMonth OneDayAfterCurrentWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.MondayDate.AddDays(1 + date - ScheduleDay.Monday).ToDayAndMonth();
        }
        
        public static DayAndMonth NextWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.NextMondayDate.AddDays(date - ScheduleDay.Monday).ToDayAndMonth();
        }
        
        public static DayAndMonth OneDayAfterNextWeekDayAndMonth(this ScheduleDay date)
        {
            if (date > ScheduleDay.Saturday)
                throw new NotSupportedException(nameof(date));
            
            return DateHelper.NextMondayDate.AddDays(1 + date - ScheduleDay.Monday).ToDayAndMonth();
        }
    }
}