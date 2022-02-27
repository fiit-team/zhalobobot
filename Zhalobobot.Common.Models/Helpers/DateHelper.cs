using System;
using System.Globalization;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Helpers
{
    public static class DateHelper
    {
        public static int WeekOfYear
            => new CultureInfo("ru-RU").Calendar.GetWeekOfYear(EkbTime, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        
        public static DateTime EkbTime => 
            DateTime.UtcNow + TimeSpan.FromHours(5);

        public static DateTime ToEkbTime(this DateTime date) =>
            date.ToUniversalTime() + TimeSpan.FromHours(5);

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
            var date = EkbTime;
            return date.DayOfWeek == DayOfWeek.Sunday 
                ? date.AddDays(-6) 
                : date.AddDays(DayOfWeek.Monday - date.DayOfWeek);
        }

        public static DateOnly ToDateOnly(this DateTime date) => DateOnly.FromDateTime(date);
        public static TimeOnly ToTimeOnly(this DateTime date) => TimeOnly.FromDateTime(date);
    }
}