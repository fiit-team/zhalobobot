using System;
using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Helpers.Helpers
{
    public static class ParsingHelper
    {
        public static Name ParseName(object lastName, object firstName, object middleName)
            => new(lastName as string ?? "", firstName as string ?? "", middleName as string);

        public static int? ParseNullableInt(object value)
            => int.TryParse(value as string, out var result) ? result : null;

        public static int ParseInt(object value)
            => int.TryParse(value as string, out var result) ? result : throw new Exception();
        
        public static long ParseLong(object value)
            => long.TryParse(value as string, out var result) ? result : throw new Exception();

        public static bool ParseBool(object value)
            => value as string == "TRUE";

        public static int? ParseCourse(object value)
        {
            if (!int.TryParse(value as string, out var admissionYear))
            {
                return null;
            }

            var zeroTime = new DateTime(1, 1, 1);
            var admissionDate = new DateTime(admissionYear, 8, 1);
            var now = DateTime.Now;
            var diff = (now - admissionDate);
            return (zeroTime + diff).Year;
        }

        public static SubjectCategory ParseSubjectCategory(object value)
        {
            var str = value as string ?? throw new ArgumentException("Subject is empty");

            return str switch
            {
                "Математика" => SubjectCategory.Math,
                "Программирование" => SubjectCategory.Programming,
                "Онлайн курсы" => SubjectCategory.OnlineCourse,
                "Другое" => SubjectCategory.Another,
                _ => throw new NotSupportedException(str)
            };
        }
        
        public static DayOfWeek ParseDay(object value)
        {
            return (value as string) switch
            {
                "Понедельник" => DayOfWeek.Monday,
                "Вторник" => DayOfWeek.Tuesday,
                "Среда" => DayOfWeek.Wednesday,
                "Четверг" => DayOfWeek.Thursday,
                "Пятница" => DayOfWeek.Friday,
                "Суббота" => DayOfWeek.Saturday,
                _ => throw new NotImplementedException()
            };
        }

        public static Month ParseMonth(object value)
        {
            return (value as string) switch
            {
                "Январь" => Month.January,
                "Февраль" => Month.February,
                "Март" => Month.March,
                "Апрель" => Month.April,
                "Май" => Month.May,
                "Июнь" => Month.June,
                "Июль" => Month.July,
                "Август" => Month.August,
                "Сентябрь" => Month.September,
                "Октябрь" => Month.October,
                "Ноябрь" => Month.November,
                "Декабрь" => Month.December,
                _ => throw new NotSupportedException()
            };
        }

        public static WeekParity ParseParity(object value)
        {
            if (value is not string str)
                return WeekParity.Both;

            if (str == "")
                return WeekParity.Both;

            if (str.Trim(' ').ToLower().StartsWith("неч"))
                return WeekParity.Odd;

            if (str.Trim(' ').ToLower().StartsWith("ч"))
                return WeekParity.Even;

            throw new Exception();
        }

        public static (DayAndMonth? DayAndMonth, HourAndMinute? HourAndMinutes)? ParseDayAndMonthOrHourAndMinutes(object value)
        {
            if (value is not string str)
                return null;

            if (str == "")
                return null;

            if (str.Contains(','))
            {
                var parts = str.Split(',');
                var dayAndMonth = parts.First(p => p.Contains('.')).Split('.');
                var hourAndMinutes = parts.First(p => p.Contains(':')).Split(':');

                var day = int.Parse(dayAndMonth[0].TrimStart('0'));
                var month = int.Parse(dayAndMonth[1].TrimStart('0'));

                if (!int.TryParse(hourAndMinutes[0].TrimStart('0'), out var hour))
                    hour = 0;
                
                if (!int.TryParse(hourAndMinutes[1].TrimStart('0'), out var minutes))
                    minutes = 0;

                return new(new DayAndMonth(day, (Month)month), new HourAndMinute(hour, minutes));
            }

            if (str.Contains(':'))
            {
                var hourAndMinutes = str.Split(':');
                
                if (!int.TryParse(hourAndMinutes[0].TrimStart('0'), out var hour))
                    hour = 0;
                
                if (!int.TryParse(hourAndMinutes[1].TrimStart('0'), out var minutes))
                    minutes = 0;
                return new ValueTuple<DayAndMonth?, HourAndMinute?>(null, new HourAndMinute(hour, minutes));
            }

            if (!str.Contains('.')) throw new Exception();
            {
                var dayAndMonth = str.Split('.');
                
                var day = int.Parse(dayAndMonth[0].TrimStart('0'));
                var month = int.Parse(dayAndMonth[1].TrimStart('0'));

                return new ValueTuple<DayAndMonth?, HourAndMinute?>(new DayAndMonth(day, (Month)month), null);
            }
        }

        public static IEnumerable<(Course, Group)> ParseFlow(object value)
        {
            if (value is not string str)
                throw new Exception();

            if (str == "")
                throw new Exception();

            var parts = str.Split(',');

            return parts.Select(part =>
            {
                var items = part
                    .Trim()
                    .Split('-', 2)[1]
                    .Split('0', 2)
                    .Select(int.Parse)
                    .ToArray();

                return ((Course)items[0], (Group)items[1]);
            });
        }

        public static IEnumerable<int> ParseRange(object value)
        {
            if (value is not string str)
                throw new Exception();

            if (str == "")
                throw new Exception();

            var parts = str.Split(',');

            foreach (var part in parts)
            {
                if (part.Contains('-'))
                {
                    var (start, end) = part.SplitPair('-');
                    for (var i = int.Parse(start); i <= int.Parse(end); i++)
                    {
                        yield return i;
                    }
                }
                else
                {
                    yield return int.Parse(part);
                }
            }
        }
    }
}