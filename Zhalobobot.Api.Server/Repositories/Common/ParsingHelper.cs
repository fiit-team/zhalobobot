using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.UserCommon;
using Group = Zhalobobot.Common.Models.Commons.Group;

namespace Zhalobobot.Api.Server.Repositories.Common
{
    public static class ParsingHelper
    {
        public static Name ParseName(object lastName, object firstName, object middleName)
            => new(lastName as string ?? "", firstName as string ?? "", middleName as string);

        public static int? ParseNullableInt(object value)
            => int.TryParse(value as string, out var result) ? result : null;

        public static int ParseInt(object value)
            => int.TryParse(value as string, out var result) ? result : throw new Exception();

        public static TEnum ParseEnum<TEnum>(object value)
            where TEnum : Enum
        {
            var integer = ParseInt(value);
            return Unsafe.As<int, TEnum>(ref integer);
        }
        
        public static long ParseLong(object value)
            => long.TryParse(value as string, out var result) ? result : throw new Exception();

        public static bool ParseBool(object value)
            => value as string == "TRUE";

        public static SubjectCategory ParseSubjectCategory(object value)
        {
            var category = value as string ?? throw new ArgumentException("Category is empty");

            return category.FromDescriptionTo<SubjectCategory>();
         }
        
        public static DayOfWeek ParseDay(object value)
        {
            return (value as string ?? "").ToLower() switch
            {
                "понедельник" => DayOfWeek.Monday,
                "вторник" => DayOfWeek.Tuesday,
                "среда" => DayOfWeek.Wednesday,
                "четверг" => DayOfWeek.Thursday,
                "пятница" => DayOfWeek.Friday,
                "суббота" => DayOfWeek.Saturday,
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

        public static (DateOnly? DateOnly, TimeOnly? TimeOnly)? ParseDateOnlyTimeOnly(object value)
        {
            if (value is not string str)
                return null;

            if (str == "")
                return null;

            if (str.Contains(','))
            {
                var (item1, item2) = str.SplitPair(',');
                var dateCorrect = DateOnly.TryParse(item1, out var date);
                if (dateCorrect)
                {
                    var timeCorrect = TimeOnly.TryParse(item2, out var time);
                    
                    if (!timeCorrect)
                        throw new Exception("Incorrect time format!");
                    
                    return (date, time);
                }
                else
                {
                    dateCorrect = DateOnly.TryParse(item2, out date);
                    var timeCorrect = TimeOnly.TryParse(item1, out var time);
                    
                    if (!timeCorrect || !dateCorrect)
                        throw new Exception("Incorrect date or time format!");
                    
                    return (date, time);
                }
            }

            var dateOnlyCorrect = DateOnly.TryParse(str, out var dateOnly);
            var timeOnlyCorrect = TimeOnly.TryParse(str, out var timeOnly);
            
            return (dateOnlyCorrect ? dateOnly : null, timeOnlyCorrect ? timeOnly : null);
        }

        private static readonly Regex ParseRegex = new(@"^ФТ-(?<course>[0-9])0(?<group>[0-9])(-(?<subgroup>[0-9]))*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static IEnumerable<(Course, Group, Subgroup?)> ParseFlow(object value)
        {
            if (value is not string str)
                throw new Exception();

            if (str == "")
                throw new Exception();

            var parts = str.Split(',');

            foreach (var part in parts)
            {
                var result = ParseRegex.Match(part.Trim());
                if (result.Success)
                {
                    var subgroup = result.Groups["subgroup"];

                    yield return (
                        (Course)int.Parse(result.Groups["course"].Value),
                        (Group)int.Parse(result.Groups["group"].Value),
                        subgroup.Success ? (Subgroup)int.Parse(subgroup.Value) : null
                    );
                }
            }
        }

        public static IEnumerable<DateOnly> ParseDateOnlyRange(object value)
        {
            if (value is not string str)
                throw new Exception();

            if (str == "")
                throw new Exception();
            
            if (str.Contains('-'))
            {
                var (start, end) = str.SplitPair('-');
                for (var i = DateOnly.Parse(start.Trim()); i <= DateOnly.Parse(end.Trim()); i = i.AddDays(1))
                    yield return i;
            }
            else
                yield return DateOnly.ParseExact(str, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        }
    }
}