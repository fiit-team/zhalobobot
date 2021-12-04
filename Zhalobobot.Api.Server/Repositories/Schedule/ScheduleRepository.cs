using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Api.Server.Repositories.Subjects;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Helpers.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public class ScheduleRepository : GoogleSheetsRepositoryBase, IScheduleRepository
    {
        private IConfiguration Configuration { get; }
        private ILogger<ScheduleRepository> Log { get; }
        
        private string ScheduleRange { get; }
        private ISubjectRepository SubjectRepository { get; }
        private bool IsFirstYearWeekOdd { get; }

        public ScheduleRepository(IConfiguration configuration, ILogger<ScheduleRepository> log, ISubjectRepository subjectRepository) 
            : base(configuration, configuration["ScheduleSpreadSheetId"])
        {
            Configuration = configuration;
            Log = log;
            // FirstCourseScheduleRange = configuration["FirstCourseScheduleRange"];
            // SecondCourseScheduleRange = configuration["SecondCourseScheduleRange"];
            ScheduleRange = configuration["ScheduleRange"];
            SubjectRepository = subjectRepository;
            IsFirstYearWeekOdd = bool.Parse(configuration["IsFirstYearWeekOdd"]);
        }

        public async Task<ScheduleItem[]> GetByCourse(Course course)
            => (await GetAll())
                .Where(s => s.Subject.Course == course)
                .ToArray();

        public async Task<ScheduleItem[]> GetByDayOfWeek(DayOfWeek dayOfWeek)
            => (await GetAll())
                .Where(schedule => schedule.EventTime.DayOfWeek == dayOfWeek)
                .ToArray();

        public async Task<ScheduleItem[]> GetByDayOfWeekAndStartsAtHourAndMinute(DayOfWeek dayOfWeek, HourAndMinute hourAndMinute)
        {
            var subjects = await GetByDayOfWeek(dayOfWeek);

            return subjects
                .Where(StartTimeMatches)
                .ToArray();

            bool StartTimeMatches(ScheduleItem item)
                => item.EventTime.Pair.HasValue && item.EventTime.Pair.Value.ToHourAndMinute().Start == hourAndMinute
                   || item.EventTime.StartTime != null && item.EventTime.StartTime == hourAndMinute;
        }
        
        public async Task<ScheduleItem[]> GetByDayOfWeekAndEndsAtHourAndMinute(DayOfWeek dayOfWeek, HourAndMinute hourAndMinute)
        {
            var subjects = await GetByDayOfWeek(dayOfWeek);

            return subjects
                .Where(EndTimeMatches)
                .ToArray();

            bool EndTimeMatches(ScheduleItem item)
                => item.EventTime.Pair.HasValue && item.EventTime.Pair.Value.ToHourAndMinute().End == hourAndMinute
                   || item.EventTime.EndTime != null && item.EventTime.EndTime == hourAndMinute;
        }

        public async Task<IEnumerable<ScheduleItem>> GetAll()
        { 
            IEnumerable<ScheduleItem> scheduleItems = await ParseScheduleRange(ScheduleRange);

            return scheduleItems.Where(s => WeekParityMatches(s) && PairExists(s));
            
            async Task<IEnumerable<ScheduleItem>> ParseScheduleRange(string scheduleRange)
            {
                var result = await GetRequest(scheduleRange).ExecuteAsync();
            
                var value = result.Values[0][0].ToString();
            
                if (value != "TRUE")
                    return Array.Empty<ScheduleItem>(); //incorrect table or synchronized == false

                var subjects = await SubjectRepository.GetAll();

                return result.Values
                    .Skip(2)
                    .SelectMany(v => ParseRow(v, subjects));
            }

            bool WeekParityMatches(ScheduleItem item)
            {
                var weekOfYear = new CultureInfo("ru-RU").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                return item.EventTime.WeekParity == WeekParity.Both
                       || IsFirstYearWeekOdd && weekOfYear % 2 == 1 && item.EventTime.WeekParity == WeekParity.Odd
                       || !IsFirstYearWeekOdd && weekOfYear % 2 == 0 && item.EventTime.WeekParity == WeekParity.Even;
            }

            bool PairExists(ScheduleItem item)
            {
                // if (item.EventTime.CanceledForever || item.EventTime.NotExistsNextTime)
                //     return false;

                var now = new DayAndMonth(DateTime.Now.Day, DateTime.Now.Month);

                if (item.EventTime.StartDay != null && item.EventTime.EndDay != null)
                    return item.EventTime.StartDay >= now && item.EventTime.EndDay <= now;
                
                if (item.EventTime.StartDay != null)
                    return item.EventTime.StartDay >= now;

                if (item.EventTime.EndDay != null)
                    return item.EventTime.EndDay <= now;

                return true;
            }
        }

        private IEnumerable<ScheduleItem> ParseRow(IList<object> row, Subject[] subjects)
        {
            var subjectName = row[2] as string ?? throw new Exception();
            var semester = SemesterHelper.Current;

            var flow = ParsingHelper.ParseFlow(row[3]).ToArray();
            
            var startDayAndMonthOrHourAndMinutes = ParsingHelper.ParseDayAndMonthOrHourAndMinutes(row[8]);
            
            var endDayAndMonthOrHourAndMinutes = ParsingHelper.ParseDayAndMonthOrHourAndMinutes(row[9]);
    
            foreach (var (course, group) in flow)
            {
                var subject = subjects.FirstOrDefault(s => s.Course == course && s.Semester == semester && s.Name == subjectName);

                if (subject == null)
                {
                    Log.LogInformation($"Subject {subjectName} not found");
                    continue;
                }
                
                var eventTime = new EventTime(
                    ParsingHelper.ParseDay(row[0]),
                    (Pair)ParsingHelper.ParseInt(row[1]),
                    startDayAndMonthOrHourAndMinutes?.HourAndMinutes,
                    endDayAndMonthOrHourAndMinutes?.HourAndMinutes,
                    startDayAndMonthOrHourAndMinutes?.DayAndMonth,
                    endDayAndMonthOrHourAndMinutes?.DayAndMonth,
                    ParsingHelper.ParseParity(row[7]));

                var subgroup = ParsingHelper.ParseNullableInt(row[4]);

                yield return new ScheduleItem(
                    subject,
                    eventTime,
                    group,
                    subgroup != null ? (Subgroup)subgroup : null,
                    row[5] as string ?? string.Empty,
                    row[6] as string ?? string.Empty);
            }
        }
    }
}