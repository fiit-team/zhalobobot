using System;
using System.Globalization;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Helpers.Helpers
{
    public static class DateHelper
    {
        public static int WeekOfYear
            => new CultureInfo("ru-RU").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

        public static WeekParity CurrentWeekParity(bool isFirstYearWeekOdd)
            => isFirstYearWeekOdd switch
            {
                false => WeekOfYear % 2 == 0 ? WeekParity.Odd : WeekParity.Even,
                true => WeekOfYear % 2 == 0 ? WeekParity.Even : WeekParity.Odd
            };

        public static WeekParity NextWeekParity(bool isFirstYearWeekOdd)
            => CurrentWeekParity(isFirstYearWeekOdd) == WeekParity.Odd
                ? WeekParity.Even
                : WeekParity.Odd;

        public static DateTime MondayDate => InternalCurrentMondayDate();
        public static DateTime NextMondayDate => InternalCurrentMondayDate().AddDays(7);
        
        private static DateTime InternalCurrentMondayDate()
        {
            var date = DateTime.Now;
            return date.DayOfWeek == DayOfWeek.Sunday 
                ? date.AddDays(-6) 
                : date.AddDays(DayOfWeek.Monday - date.DayOfWeek);
        }

        public static DayAndMonth ToDayAndMonth(this DateTime date) => new(date.Day, (Month)date.Month);
        public static HourAndMinute ToHourAndMinute(this DateTime date) => new(date.Hour, date.Minute);
    }
}