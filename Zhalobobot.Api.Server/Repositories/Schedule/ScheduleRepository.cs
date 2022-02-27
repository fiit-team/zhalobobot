using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Api.Server.Repositories.Subjects;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public class ScheduleRepository : GoogleSheetsRepositoryBase, IScheduleRepository
    {
        private string ScheduleRange { get; }
        private string HolidaysRange { get; }
        private ISubjectRepository SubjectRepository { get; }
        private ILogger<ScheduleRepository> Log { get; }

        public ScheduleRepository(ISubjectRepository subjectRepository, IConfiguration configuration, ILogger<ScheduleRepository> log) 
            : base(configuration, configuration["ScheduleSpreadSheetId"])
        {
            ScheduleRange = configuration["ScheduleRange"];
            HolidaysRange = configuration["HolidaysRange"];
            SubjectRepository = subjectRepository;
            Log = log;
        }

        public async Task<IEnumerable<ScheduleItem>> GetAll()
        { 
            IEnumerable<ScheduleItem> scheduleItems = await ParseScheduleRange(ScheduleRange);

            return scheduleItems.Where(PairExists);
            
            async Task<IEnumerable<ScheduleItem>> ParseScheduleRange(string scheduleRange)
            {
                var result = await GetRequest(scheduleRange).ExecuteAsync();
            
                var checkbox = result.Values[0][0].ToString() ?? "";
            
                if (checkbox.ToLower() != "true")
                    return Array.Empty<ScheduleItem>(); //incorrect table or synchronized == false

                var subjects = await SubjectRepository.GetAll();

                return result.Values
                    .Skip(2)
                    .SelectMany(v => ParseRow(v, subjects));
            }

            bool PairExists(ScheduleItem item)
            {
                var now = new DayAndMonth(DateHelper.EkbTime.Day, (Month)DateHelper.EkbTime.Month);

                if (item.EventTime.StartDay != null && item.EventTime.EndDay != null)
                    return item.EventTime.StartDay >= now && item.EventTime.EndDay <= now;
                
                if (item.EventTime.StartDay != null)
                    return item.EventTime.StartDay >= now;

                if (item.EventTime.EndDay != null)
                    return item.EventTime.EndDay <= now;

                return true;
            }
        }

        public async Task<DateOnly[]> GetHolidays()
        {
            var result = await GetRequest(HolidaysRange).ExecuteAsync();

            return result.Values
                .SelectMany(row => ParsingHelper.ParseDateOnlyRange(row[0]))
                .ToArray();
        }

        private IEnumerable<ScheduleItem> ParseRow(IList<object> row, Subject[] subjects)
        {
            var subjectName = row[2] as string ?? throw new Exception();
            var semester = SemesterHelper.Current;

            var flow = ParsingHelper.ParseFlow(row[3]).ToArray();
            
            var startDayOrTime = ParsingHelper.ParseDateOnlyTimeOnly(row[8]);
            
            var endDayOrTime = ParsingHelper.ParseDateOnlyTimeOnly(row[9]);
    
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
                    ParsingHelper.ParseEnum<Pair>(row[1]),
                    startDayOrTime?.TimeOnly,
                    endDayOrTime?.TimeOnly,
                    startDayOrTime?.DateOnly,
                    endDayOrTime?.DateOnly,
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