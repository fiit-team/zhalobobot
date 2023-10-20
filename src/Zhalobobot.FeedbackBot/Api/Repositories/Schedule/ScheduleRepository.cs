// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using Vostok.Hosting.Abstractions;
// using Vostok.Hosting.Abstractions.Requirements;
// using Zhalobobot.Bot.Api.Repositories.Common;
// using Zhalobobot.Bot.Api.Repositories.Subjects;
// using Zhalobobot.Bot.Settings;
// using Zhalobobot.Common.Models.Commons;
// using Zhalobobot.Common.Models.Helpers;
// using Zhalobobot.Common.Models.Schedule;
// using Zhalobobot.Common.Models.Subject;
//
// namespace Zhalobobot.Bot.Api.Repositories.Schedule;
//
// [RequiresSecretConfiguration(typeof(BotSecrets))]
// public class ScheduleRepository : GoogleSheetsRepositoryBase, IScheduleRepository
// {
//     private string ScheduleRange { get; }
//     private string DayWithoutPairsRange { get; }
//     private ISubjectRepository SubjectRepository { get; }
//     private ILogger<ScheduleRepository> Log { get; }
//
//     public ScheduleRepository(ISubjectRepository subjectRepository, IVostokHostingEnvironment environment, ILogger<ScheduleRepository> log) 
//         : base(environment, environment.SecretConfigurationProvider.Get<BotSecrets>().ScheduleSpreadSheetId)
//     {
//         ScheduleRange = environment.SecretConfigurationProvider.Get<BotSecrets>().ScheduleRange;
//         DayWithoutPairsRange = environment.SecretConfigurationProvider.Get<BotSecrets>().DayWithoutPairsRange;
//         SubjectRepository = subjectRepository;
//         Log = log;
//     }
//
//     public async Task<Zhalobobot.Common.Models.Schedule.Schedule> GetAll()
//     { 
//         var (shouldBeUpdated, scheduleItems) = await ParseScheduleRange(ScheduleRange);
//
//         return shouldBeUpdated
//             ? new(true, scheduleItems.ToArray())
//             : new(false, Array.Empty<ScheduleItem>());
//
//         async Task<(bool ShouldBeUpdated, IEnumerable<ScheduleItem> Items)> ParseScheduleRange(string scheduleRange)
//         {
//             var result = await GetRequest(scheduleRange).ExecuteAsync();
//             
//             var shouldUpdate = ParsingHelper.ParseBool(result.Values[0][0]);
//             
//             if (!shouldUpdate)
//                 return (false, Array.Empty<ScheduleItem>()); //incorrect table or synchronized == false
//
//             var subjects = await SubjectRepository.GetAll();
//
//             return (true, result.Values
//                 .Skip(2)
//                 .SelectMany(v => ParseRow(v, subjects)));
//         }
//     }
//
//     public async Task<DayWithoutPairs[]> GetDaysWithoutPairs()
//     {
//         var result = await GetRequest(DayWithoutPairsRange).ExecuteAsync();
//         var subjects = await SubjectRepository.GetAll();
//
//         return result.Values
//             .SelectMany(row => ParseDayWithoutPairs(row, subjects))
//             .Distinct()
//             .ToArray();
//     }
//
//     private static IEnumerable<DayWithoutPairs> ParseDayWithoutPairs(IList<object> row, Subject[] subjects)
//     {
//         var startTimes = row.Count >= 4
//             ? ParsingHelper.ParseTimeOnlyRange(row[3]).ToArray()
//             : null;
//
//         var dates = ParsingHelper.ParseDateOnlyRange(row[0]);
//
//         var subjectName = row[1] as string ?? throw new Exception("Invalid subject name");
//             
//         foreach (var date in dates)
//         foreach (var (course, group, subgroup) in ParsingHelper.ParseStudyGroups(row[2]))
//         {
//             var possibleSubgroups = subgroup.HasValue 
//                 ? new[] { subgroup.Value } 
//                 : Enum.GetValues<Subgroup>().ToArray();
//
//             foreach (var possibleSubgroup in possibleSubgroups)
//             {
//                 var subjectNames = subjectName.ToLowerInvariant() == "все"
//                     ? subjects.Where(s => s.Course == course).Select(s => s.Name).ToArray()
//                     : new[] { subjectName };
//                     
//                 foreach (var name in subjectNames)
//                 {
//                     if (startTimes != null)
//                         foreach (var startTime in startTimes)
//                             yield return new DayWithoutPairs(date, name, course, group, possibleSubgroup, startTime);
//                     else
//                         yield return new DayWithoutPairs(date, name, course, group, possibleSubgroup, null);
//                 }
//             }
//         }
//     }
//
//     private IEnumerable<ScheduleItem> ParseRow(IList<object> row, Subject[] subjects)
//     {
//         var subjectName = row[2] as string ?? throw new Exception();
//         var semester = SemesterHelper.Current;
//
//         var flow = ParsingHelper.ParseStudyGroups(row[3]).ToArray();
//             
//         var startDayOrTime = ParsingHelper.ParseDateOnlyTimeOnly(row[8]);
//             
//         var endDayOrTime = ParsingHelper.ParseDateOnlyTimeOnly(row[9]);
//     
//         foreach (var (course, group, flowSubgroup) in flow)
//         {
//             var subject = subjects.FirstOrDefault(s => s.Course == course && s.Semester == semester && s.Name == subjectName);
//
//             if (subject == null)
//             {
//                 Log.LogInformation($"Subject {subjectName} not found");
//                 continue;
//             }
//                 
//             var eventTime = new EventTime(
//                 ParsingHelper.ParseDay(row[0]),
//                 ParsingHelper.ParseEnum<Pair>(row[1]),
//                 startDayOrTime.TimeOnly,
//                 endDayOrTime.TimeOnly,
//                 startDayOrTime.DateOnly,
//                 endDayOrTime.DateOnly,
//                 ParsingHelper.ParseParity(row[7]));
//
//             var subgroup = GetSubgroup(flowSubgroup, ParsingHelper.ParseNullableInt(row[4]));
//
//             var subgroups = subgroup.HasValue ? new[] { subgroup.Value } : Enum.GetValues<Subgroup>();
//
//             foreach (var sub in subgroups)
//             {
//                 yield return new ScheduleItem(
//                     subject,
//                     eventTime,
//                     group,
//                     sub,
//                     row[5] as string ?? string.Empty,
//                     row[6] as string ?? string.Empty);
//             }
//
//             static Subgroup? GetSubgroup(Subgroup? flowSubgroup, int? subgroupValue)
//             {
//                 if (flowSubgroup.HasValue)
//                     return flowSubgroup.Value;
//                 if (subgroupValue.HasValue)
//                     return (Subgroup)subgroupValue.Value;
//                 return null;
//             }
//         }
//     }
// }