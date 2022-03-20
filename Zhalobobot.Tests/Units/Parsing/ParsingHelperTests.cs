using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Tests.Units.Parsing;

[TestFixture]
public class ParsingHelperTests
{
    [TestCaseSource(nameof(GetFlowData))]
    public void Should_parse_flow(string flow, (Course, Group, Subgroup?)[] result)
    {
        ParsingHelper.ParseFlow(flow).Should().BeEquivalentTo(result);
    }

    [TestCaseSource(nameof(GetDateOnlyRangeData))]
    public void Should_parse_dateOnlyRange(string dateOnlyRange, DateOnly[] result)
    {
        ParsingHelper.ParseDateOnlyRange(dateOnlyRange).Should().BeEquivalentTo(result);
    }

    [TestCaseSource(nameof(GetTimeOnlyRangeData))]
    public void Should_parse_timeOnlyRange(string timeOnlyRange, TimeOnly[] result)
    {
        ParsingHelper.ParseTimeOnlyRange(timeOnlyRange).Should().BeEquivalentTo(result);
    }

    [TestCaseSource(nameof(GetDateOnlyTimeOnlyData))]
    public void Should_parse_dateOnlyTimeOnly(string dateOnlyTimeOnly, (DateOnly?, TimeOnly?) result)
    {
        ParsingHelper.ParseDateOnlyTimeOnly(dateOnlyTimeOnly).Should().BeEquivalentTo(result);
    }

    [TestCaseSource(nameof(GetParityData))]
    public void Should_parse_parity(string parity, WeekParity result)
    {
        ParsingHelper.ParseParity(parity).Should().Be(result);
    }

    [TestCaseSource(nameof(GetDayOfWeekData))]
    public void Should_parse_dayOfWeek(string day, DayOfWeek result)
    {
        ParsingHelper.ParseDay(day).Should().Be(result);
    }

    private static IEnumerable<TestCaseData> GetDayOfWeekData()
    {
        yield return new TestCaseData("Понедельник", DayOfWeek.Monday)
        {
            TestName = "Monday"
        };
        yield return new TestCaseData("Вторник", DayOfWeek.Tuesday)
        {
            TestName = "Tuesday"
        };
        yield return new TestCaseData("Среда", DayOfWeek.Wednesday)
        {
            TestName = "Wednesday"
        };
        yield return new TestCaseData("Четверг", DayOfWeek.Thursday)
        {
            TestName = "Thursday"
        };
        yield return new TestCaseData("Пятница", DayOfWeek.Friday)
        {
            TestName = "Friday"
        };
        yield return new TestCaseData("Суббота", DayOfWeek.Saturday)
        {
            TestName = "Saturday"
        };
    }

    private static IEnumerable<TestCaseData> GetParityData()
    {
        yield return new TestCaseData("ч", WeekParity.Even)
        {
            TestName = "Even short"
        };
        yield return new TestCaseData("четная", WeekParity.Even)
        {
            TestName = "Even full"
        };
        yield return new TestCaseData("неч", WeekParity.Odd)
        {
            TestName = "Odd short"
        };
        yield return new TestCaseData("нечетная", WeekParity.Odd)
        {
            TestName = "Odd full"
        };
        yield return new TestCaseData("", WeekParity.Both)
        {
            TestName = "Both"
        };
    }

    private static IEnumerable<TestCaseData> GetDateOnlyTimeOnlyData()
    {
        yield return new TestCaseData("14:30", new ValueTuple<DateOnly?, TimeOnly?>(null, new TimeOnly(14, 30)))
        {
            TestName = "TimeOnly"
        };
        yield return new TestCaseData("13.01.2022", new ValueTuple<DateOnly?, TimeOnly?>(new DateOnly(2022, 1, 13), null))
        {
            TestName = "DateOnly"
        };
        yield return new TestCaseData("13.01.2022, 14:30", new ValueTuple<DateOnly?, TimeOnly?>(new DateOnly(2022, 1, 13), new TimeOnly(14, 30)))
        {
            TestName = "DateOnly and TimeOnly"
        };
        yield return new TestCaseData("14:30, 13.01.2022", new ValueTuple<DateOnly?, TimeOnly?>(new DateOnly(2022, 1, 13), new TimeOnly(14, 30)))
        {
            TestName = "TimeOnly and DateOnly"
        };
    }

    private static IEnumerable<TestCaseData> GetTimeOnlyRangeData()
    {
        yield return new TestCaseData(
            "14:30",
            new[]
            {
                new TimeOnly(14, 30)
            })
        {
            TestName = "Time"
        };
        yield return new TestCaseData(
            "14:30, 15:30",
            new[]
            {
                new TimeOnly(14, 30),
                new TimeOnly(15, 30)
            })
        {
            TestName = "Multiple times"
        };
    }

    private static IEnumerable<TestCaseData> GetDateOnlyRangeData()
    {
        yield return new TestCaseData(
            "13.01.2022",
            new[]
            {
                new DateOnly(2022, 1, 13)
            })
        {
            TestName = "Day"
        };
        yield return new TestCaseData(
            "13.01.2022, 15.01.2022",
            new[]
            {
                new DateOnly(2022, 1, 13),
                new DateOnly(2022, 1, 15)
            })
        {
            TestName = "Multiple days"
        };
        yield return new TestCaseData(
            "13.01.2022 - 15.01.2022",
            new[]
            {
                new DateOnly(2022, 1, 13),
                new DateOnly(2022, 1, 14),
                new DateOnly(2022, 1, 15)
            })
        {
            TestName = "Days range"
        };
        yield return new TestCaseData(
            "13.01.2022 - 15.01.2022, 16.01.2022",
            new[]
            {
                new DateOnly(2022, 1, 13),
                new DateOnly(2022, 1, 14),
                new DateOnly(2022, 1, 15),
                new DateOnly(2022, 1, 16)
            })
        {
            TestName = "Day and days range"
        };
    }

    private static IEnumerable<TestCaseData> GetFlowData()
    {
        yield return new TestCaseData(
            "ФТ-101",
            new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.First, null)
            })
        {
            TestName = "Course with group"
        };
        yield return new TestCaseData(
            "ФТ-101-1",
            new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.First, Subgroup.First)
            })
        {
            TestName = "Course with group and subgroup"
        };
        yield return new TestCaseData(
            "ФТ-101, ФТ-102, ФТ-302",
            new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.First, null),
                (Course.First, Group.Second, null),
                (Course.Third, Group.Second, null)
            })
        {
            TestName = "Multiple courses with group"
        };
        yield return new TestCaseData(
            "ФТ-101-1, ФТ-202-2, ФТ-301-2",
            new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.First, Subgroup.First),
                (Course.Second, Group.Second, Subgroup.Second),
                (Course.Third, Group.First, Subgroup.Second)
            })
        {
            TestName = "Multiple courses with group and subgroup"
        };
        yield return new TestCaseData(
            "ФТ-101-1, ФТ-202",
            new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.First, Subgroup.First),
                (Course.Second, Group.Second, null)
            })
        {
            TestName = "Course with group and course with group and subgroup"
        };
        yield return new TestCaseData(
            "1 курс",
            GenerateForCourse(Course.First).ToArray())
        {
            TestName = "Course"
        };
        yield return new TestCaseData(
            "2курс, 3 курс",
            GenerateForCourse(Course.Second).Concat(GenerateForCourse(Course.Third)).ToArray())
        {
            TestName = "Multiple courses"
        };
        yield return new TestCaseData(
            "1 курс, ФТ-102",
            GenerateForCourse(Course.First).Concat(new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.Second, null)
            }).ToArray())
        {
            TestName = "Course and course with group"
        };
        yield return new TestCaseData(
            "1 курс, ФТ-102-2",
            GenerateForCourse(Course.First).Concat(new (Course, Group, Subgroup?)[]
            {
                (Course.First, Group.Second, Subgroup.Second)
            }).ToArray())
        {
            TestName = "Course and course with group and subgroup"
        };
        yield return new TestCaseData(
            "все",
            Enum.GetValues<Course>().SelectMany(GenerateForCourse).ToArray())
        {
            TestName = "All"
        };

        IEnumerable<(Course, Group, Subgroup?)> GenerateForCourse(Course course)
        {
            foreach (var group in Enum.GetValues<Group>())
            foreach (var subgroup in Enum.GetValues<Subgroup>())
                yield return (course, group, subgroup);
        }
    }
}