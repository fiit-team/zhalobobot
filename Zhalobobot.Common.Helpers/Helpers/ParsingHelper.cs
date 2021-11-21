using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool ParseBool(object value)
            => value as string == "TRUE";

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

                return new(new DayAndMonth(day, month), new HourAndMinute(hour, minutes));
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

                return new ValueTuple<DayAndMonth?, HourAndMinute?>(new DayAndMonth(day, month), null);
            }
        }

        public static IEnumerable<int> ParseRange(object value)
        {
            if (value is not string str)
                throw new Exception();

            if (str == "")
                throw new Exception();

            var parts = str.Split(',');

            return parts.SelectMany(p =>
            {
                var range = p.Split('-').Select(s => int.Parse(s.Trim(' '))).ToArray();
                if (range.Length == 1)
                    return new List<int> { range[0] };

                var minValue = range.Min();
                var maxValue = range.Max();
                
                var res = new List<int>();
                for (var i = minValue; i <= maxValue; i++)
                    res.Add(i);
                
                return res;
            });
        }
    }
}