// using System;
// using System.Collections.Generic;
// using FluentAssertions;
// using NUnit.Framework;
// using Zhalobobot.Common.Helpers.Extensions;
// using Zhalobobot.Common.Models.Commons;
// using Zhalobobot.Common.Models.Helpers;
// using Zhalobobot.Common.Models.Schedule;
// using Zhalobobot.Common.Models.Subject;
//
// namespace Zhalobobot.Tests.Units.Common.Extensions;
//
// [TestFixture]
// public class ScheduleItemExtensionsTests
// {
//     private static readonly DateTime DateDefault = new(2022, 3, 10, 10, 30, 0);
//     private const Course CourseDefault = Course.First;
//     private const Group GroupDefault = Group.First;
//     private const Subgroup SubgroupDefault = Subgroup.First;
//     
//     [TestCaseSource(nameof(GetLastStudyWeekDayData))]
//     public void Should_return_correct_lastStudyWeekDay(IEnumerable<ScheduleItem> items, DayOfWeek result)
//     {
//         items.LastStudyWeekDay().Should().Be(result);
//     }
//
//     [Test]
//     public void Should_filter_scheduleItems_when_they_match_with_dayWithoutPairs_without_end_time()
//     {
//         const string subjectName = "subjectName";
//
//         var daysWithoutPair = new[]
//         {
//             GenerateDayWithoutPair(subjectName)
//         };
//
//         var filteredItem = GenerateScheduleItem(subjectName);
//
//         var notFilteredItem = GenerateScheduleItem();
//
//         var scheduleItems = new[] { notFilteredItem, filteredItem };
//
//         scheduleItems.ActualDaySchedule(DateDefault, daysWithoutPair, false)
//             .Should().BeEquivalentTo(new[] { notFilteredItem }, c => c.WithoutStrictOrdering());
//     }
//     
//     [Test]
//     public void Should_filter_scheduleItems_when_they_match_with_dayWithoutPairs_with_end_time()
//     {
//         const string subjectName = "subjectName";
//         var endTime = DateDefault.ToTimeOnly();
//
//         var daysWithoutPair = new[]
//         {
//             GenerateDayWithoutPair(subjectName, endTime: endTime)
//         };
//
//         var filteredItem = GenerateScheduleItem(subjectName, endTime: endTime);
//
//         var notFilteredItem = GenerateScheduleItem(endTime: endTime);
//
//         var scheduleItems = new[] { notFilteredItem, filteredItem };
//
//         scheduleItems.ActualDaySchedule(DateDefault, daysWithoutPair, false)
//             .Should().BeEquivalentTo(new[] { notFilteredItem });
//     }
//
//     [TestCaseSource(nameof(GetTakeCorrectItemData))]
//     public void Should_take_correct_item(ScheduleItem filteredItem, ScheduleItem notFilteredItem)
//     {
//         var scheduleItems = new[] { filteredItem, notFilteredItem };
//
//         scheduleItems.ActualDaySchedule(DateDefault, Array.Empty<DayWithoutPairs>(), false)
//             .Should().BeEquivalentTo(new[] { notFilteredItem });
//     }
//
//     [TestCaseSource(nameof(GetItemsWithSameNameStartsAndEndsInSameTimeData))]
//     public void Should_take_correct_item_when_have_items_with_same_name_starts_and_ends_in_same_time(
//         DateOnly? filteredItemStartDay, 
//         DateOnly? filteredItemEndDay,
//         DateOnly? notFilteredItemStartDay, 
//         DateOnly? notFilteredItemEndDay)
//     {
//         const string subjectName = "subjectName";
//
//         var filteredItem = GenerateScheduleItem(subjectName, startDay: filteredItemStartDay, endDay: filteredItemEndDay);
//         var notFilteredItem = GenerateScheduleItem(subjectName, startDay: notFilteredItemStartDay, endDay: notFilteredItemEndDay);
//
//         var scheduleItems = new[] { notFilteredItem, filteredItem };
//
//         scheduleItems.ActualDaySchedule(DateDefault, Array.Empty<DayWithoutPairs>(), false)
//             .Should().BeEquivalentTo(new[] { notFilteredItem });
//     }
//     
//     [Test]
//     public void Should_take_all_items_if_skipEndTimeCheck_is_true()
//     {
//         var notFilteredItem1 = GenerateScheduleItem(endTime: DateDefault.ToTimeOnly());
//         var notFilteredItem2 = GenerateScheduleItem(endTime: DateDefault.AddHours(3).ToTimeOnly());
//
//         var scheduleItems = new[] { notFilteredItem1, notFilteredItem2 };
//
//         scheduleItems.ActualDaySchedule(DateDefault, Array.Empty<DayWithoutPairs>(), true)
//             .Should().BeEquivalentTo(new[] { notFilteredItem1, notFilteredItem2 }, c => c.WithoutStrictOrdering());
//     }
//
//     private static IEnumerable<TestCaseData> GetItemsWithSameNameStartsAndEndsInSameTimeData()
//     {
//         yield return new TestCaseData(DateDefault.AddDays(-7).ToDateOnly(), DateDefault.ToDateOnly(), DateDefault.ToDateOnly(), DateDefault.ToDateOnly())
//         {
//             TestName = "Should take item with latest StartDay when both have StartDay and EndDay"
//         };
//         yield return new TestCaseData(DateDefault.AddDays(-7).ToDateOnly(), null, DateDefault.ToDateOnly(), null)
//         {
//             TestName = "Should take item with latest StartDay when both have only StartDay"
//         };
//         yield return new TestCaseData(null, DateDefault.AddDays(7).ToDateOnly(), null, DateDefault.ToDateOnly())
//         {
//             TestName = "Should take item with earliest EndDay when both have only EndDay"
//         };
//         yield return new TestCaseData(null, DateDefault.ToDateOnly(), DateDefault.ToDateOnly(), null)
//         {
//             TestName = "Should take item with StartDay when one have StartDay and another have EndDay"
//         };
//         yield return new TestCaseData(null, null, null, null)
//         {
//             TestName = "Should take first item when both have not StartDay and EndDay"
//         };
//     }
//     
//     private static IEnumerable<TestCaseData> GetTakeCorrectItemData()
//     {
//         yield return new TestCaseData(
//             GenerateScheduleItem(startDay: DateDefault.AddDays(1).ToDateOnly()),
//             GenerateScheduleItem(startDay: DateDefault.ToDateOnly())
//         )
//         {
//             TestName = "Should filter scheduleItem when StartDay after current date"
//         };
//         yield return new TestCaseData(
//             GenerateScheduleItem(endDay: DateDefault.AddDays(-1).ToDateOnly()),
//             GenerateScheduleItem(endDay: DateDefault.ToDateOnly())
//         )
//         {
//             TestName = "Should filter scheduleItem when EndDay before current date"
//         };
//         yield return new TestCaseData(
//             GenerateScheduleItem(dayOfWeek: DateDefault.AddDays(1).DayOfWeek),
//             GenerateScheduleItem(dayOfWeek: DateDefault.DayOfWeek)
//         )
//         {
//             TestName = "Should filter scheduleItem when dayOfWeek not match"
//         };
//         yield return new TestCaseData(
//             GenerateScheduleItem(endTime: DateDefault.ToTimeOnly().AddHours(1)),
//             GenerateScheduleItem(endTime: DateDefault.ToTimeOnly())
//         )
//         {
//             TestName = "Should filter scheduleItem when EndTime not match"
//         };
//     }
//
//     private static IEnumerable<TestCaseData> GetLastStudyWeekDayData()
//     {
//         yield return new TestCaseData(
//             new[]
//             {
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Monday),
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Tuesday),
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Friday)
//                 
//             },
//             DayOfWeek.Friday
//         )
//         {
//             TestName = "Return correct day if item's days in sorted order"
//         };
//         yield return new TestCaseData(
//             new[]
//             {
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Tuesday),
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Monday),
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Saturday),
//                 GenerateScheduleItem(dayOfWeek: DayOfWeek.Friday)
//             },
//             DayOfWeek.Saturday
//         )
//         {
//             TestName = "Return correct day if item's days is random order"
//         };
//     }
//
//     private static DayWithoutPairs GenerateDayWithoutPair(
//         string? subjectName = null,
//         DateOnly? date = null,
//         Course? course = null,
//         Group? group = null,
//         Subgroup? subgroup = null,
//         TimeOnly? endTime = null
//     )
//     {
//         return new DayWithoutPairs(
//             date ?? DateDefault.ToDateOnly(),
//             subjectName ?? Guid.NewGuid().ToString("N"),
//             course ?? CourseDefault,
//             group ?? GroupDefault,
//             subgroup ?? SubgroupDefault,
//             endTime);
//     }
//     
//     private static ScheduleItem GenerateScheduleItem(
//         string? subjectName = null,
//         DayOfWeek? dayOfWeek = null,
//         Course? course = null,
//         Semester? semester = null,
//         Group? group = null,
//         Subgroup? subgroup = null,
//         TimeOnly? startTime = null,
//         TimeOnly? endTime = null,
//         DateOnly? startDay = null,
//         DateOnly? endDay = null,
//         int studentsToNotifyPercent = 0)
//     {
//         var subject = new Subject(
//             subjectName ?? Guid.NewGuid().ToString("N"),
//             course ?? CourseDefault,
//             semester ?? Semester.First,
//             SubjectCategory.Math,
//             studentsToNotifyPercent);
//
//         var eventTime = new EventTime(
//             dayOfWeek ?? DateDefault.DayOfWeek, 
//             Pair.First, 
//             startTime, 
//             endTime ?? DateDefault.ToTimeOnly(), 
//             startDay, 
//             endDay, 
//             WeekParity.Both);
//         
//         return new ScheduleItem(subject, eventTime, group ?? GroupDefault, subgroup ?? SubgroupDefault, "123", "someone");
//     }
// }