using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Helpers.Helpers;
using Zhalobobot.Common.Models.Serialization;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public class ScheduleRepository : GoogleSheetsRepositoryBase, IScheduleRepository
    {
        private IConfiguration Configuration { get; }
        private ILogger<ScheduleRepository> Logger { get; }
        private string ScheduleTestRange { get; }

        public ScheduleRepository(IConfiguration configuration, ILogger<ScheduleRepository> logger) 
            : base(configuration, configuration["FeedbackSpreadSheetId"])
        {
            Configuration = configuration;
            Logger = logger;
            ScheduleTestRange = configuration["ScheduleTestRange"];
        }

        public async Task<ScheduleItem[]?> ParseSchedule()
        {
            var result = await GetRequest(ScheduleTestRange).ExecuteAsync();

            Logger.LogInformation(result.Values.ToPrettyJson());
            
            var value = result.Values[0][0].ToString();
            
            if (value != "TRUE" && value != "FALSE")
                return null; //incorrect table
            
            var a = result.Values.Skip(2).Select(ParseRow).ToArray();
            
            Logger.LogInformation(a.ToPrettyJson());

            return a;
        }

        private static ScheduleItem ParseRow(IList<object> row)
        {
            return new ScheduleItem(
                ParsingHelper.ParseDay(row[0]),
                ParsingHelper.ParseInt(row[1]),
                new OldSubject(row[2] as string ?? string.Empty, ParsingHelper.ParseSubjectCategory(row[7])),
                ParsingHelper.ParseInt(row[3]),
                ParsingHelper.ParseNullableInt(row[4]),
                row[5] as string ?? string.Empty, //ParsingHelper.ParseNullableInt(row[5]),
                row[6] as string ?? string.Empty, //ParsingHelper.ParseNullableInt(row[6]),
                row[8] as string ?? string.Empty,
                row[9] as string ?? string.Empty,
                ParsingHelper.ParseBool(row[10]),
                ParsingHelper.ParseBool(row[11]),
                ParsingHelper.ParseBool(row[12]),
                ParsingHelper.ParseBool(row[13]),
                ParsingHelper.ParseBool(row[14]));
        }
    }

    public record ScheduleItem(
        DayOfWeek DayOfWeek,
        int Para,
        OldSubject OldSubject,
        int GroupNumber,
        int? SubgroupNumber,
        string StartTime,
        string EndTime,
        string Prepod,
        string Cabinet,
        bool CanceledCurrentWeek,
        bool CanceledNextWeed,
        bool ExistsOnlyCurrentWeek,
        bool ExistsOnlyNextWeek,
        bool CanceledForever);
}