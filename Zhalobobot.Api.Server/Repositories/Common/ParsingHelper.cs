using System;
using System.Collections.Generic;
using System.Linq;
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
        public static Name ParseName(object lastName, object firstName, object? middleName)
            => new(lastName as string ?? "", firstName as string ?? "", middleName as string);

        public static string[] ParseSpecialCourseNames(object? specialCourses)
            => specialCourses == null 
                ? Array.Empty<string>() 
                : (specialCourses as string ?? "").Split(';').Select(sc => sc.Trim()).ToArray();

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

        public static (DateOnly? DateOnly, TimeOnly? TimeOnly) ParseDateOnlyTimeOnly(object value)
        {
            var str = EnsureCorrectString(value);

            str = str.Trim();

            if (str.Contains(','))
            {
                var (item1, item2) = str.SplitPair(',');
                item1 = item1.Trim();
                item2 = item2.Trim();
                var dateCorrect = DateOnly.TryParseExact(item1, "dd.MM.yyyy", out var date);
                if (dateCorrect)
                {
                    var timeCorrect = TimeOnly.TryParse(item2, out var time);
                    
                    if (!timeCorrect)
                        throw new Exception("Incorrect time format!");
                    
                    return (date, time);
                }
                else
                {
                    dateCorrect = DateOnly.TryParseExact(item2, "dd.MM.yyyy", out date);
                    var timeCorrect = TimeOnly.TryParse(item1, out var time);
                    
                    if (!timeCorrect || !dateCorrect)
                        throw new Exception($"Incorrect date or time format! Date: {date}, Time: {time}");
                    
                    return (date, time);
                }
            }

            var dateOnlyCorrect = DateOnly.TryParseExact(str, "dd.MM.yyyy", out var dateOnly);
            var timeOnlyCorrect = TimeOnly.TryParse(str, out var timeOnly);
            
            return (dateOnlyCorrect ? dateOnly : null, timeOnlyCorrect ? timeOnly : null);
        }

        private static readonly Regex CourseGroupSubgroupRegex = new(@"^ФТ-(?<course>[0-9])0(?<group>[0-9])(-(?<subgroup>[0-9]))*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CourseRegex = new(@"(?<course>[0-9])\s*курс", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static IEnumerable<(Course, Group, Subgroup?)> ParseFlow(object value)
        {
            var str = EnsureCorrectString(value);

            var parts = str.Split(',');

            if (parts.Any(part => part.ToLower() == "все"))
            {
                foreach (var course in Enum.GetValues<Course>())
                foreach (var group in Enum.GetValues<Group>())
                foreach (var subgroup in Enum.GetValues<Subgroup>())
                    yield return (course, group, subgroup);
                yield break;
            }

            foreach (var part in parts)
            {
                var result = CourseRegex.Match(part.Trim());
                if (result.Success)
                {
                    var course = (Course)int.Parse(result.Groups["course"].Value);
                    foreach (var group in Enum.GetValues<Group>())
                    foreach (var subgroup in Enum.GetValues<Subgroup>())
                        yield return (course, group, subgroup);
                }
                
                result = CourseGroupSubgroupRegex.Match(part.Trim());
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
            var str = EnsureCorrectString(value);

            return ParseDateOnlyRangeInternal(str);
        }

        public static IEnumerable<TimeOnly> ParseTimeOnlyRange(object value)
        {
            var str = EnsureCorrectString(value);

            var parts = str.Split(",");

            foreach (var part in parts)
                yield return TimeOnly.Parse(part.Trim());
        }

        private static IEnumerable<DateOnly> ParseDateOnlyRangeInternal(string str, string format = "dd.MM.yyyy")
        {
            var parts = str.Split(',');
            foreach (var part in parts)
            {
                if (part.Contains('-'))
                {
                    var (start, end) = part.SplitPair('-');
                    for (var i = DateOnly.ParseExact(start.Trim(), format); i <= DateOnly.ParseExact(end.Trim(), format); i = i.AddDays(1))
                        yield return i;
                }
                else
                    yield return DateOnly.ParseExact(part.Trim(), format);
            }
        }

        private static string EnsureCorrectString(object value)
        {
            if (value is not string str)
                throw new Exception();

            if (str == "")
                throw new Exception();

            return str;
        }
    }
}