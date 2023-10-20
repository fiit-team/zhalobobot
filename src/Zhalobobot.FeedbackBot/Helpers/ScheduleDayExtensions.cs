// using System;
// using Zhalobobot.Bot.Models;
// using Zhalobobot.Common.Models.Helpers;
//
// namespace Zhalobobot.Bot.Helpers
// {
//     public static class ScheduleDayExtensions
//     {
//         public static bool IsCurrentWeek(this ScheduleDay day) =>
//             day is not (ScheduleDay.NextMonday or ScheduleDay.NextWeek);
//
//         public static DateOnly CurrentWeekDayAndMonth(this ScheduleDay date)
//         {
//             EnsureCorrect(date);
//             
//             return DateHelper.MondayDate.AddDays(date - ScheduleDay.Monday).ToDateOnly();
//         }
//
//         public static DateOnly OneDayAfterCurrentWeekDayAndMonth(this ScheduleDay date)
//         {
//             EnsureCorrect(date);
//             
//             return DateHelper.MondayDate.AddDays(1 + date - ScheduleDay.Monday).ToDateOnly();
//         }
//         
//         public static DateOnly NextWeekDayAndMonth(this ScheduleDay date)
//         {
//             EnsureCorrect(date);
//             
//             return DateHelper.NextMondayDate.AddDays(date - ScheduleDay.Monday).ToDateOnly();
//         }
//         
//         public static DateOnly OneDayAfterNextWeekDayAndMonth(this ScheduleDay date)
//         {
//             EnsureCorrect(date);
//             
//             return DateHelper.NextMondayDate.AddDays(1 + date - ScheduleDay.Monday).ToDateOnly();
//         }
//
//         private static void EnsureCorrect(ScheduleDay date)
//         {
//             if (date > ScheduleDay.Sunday)
//                 throw new NotSupportedException(nameof(date));
//         }
//     }
// }