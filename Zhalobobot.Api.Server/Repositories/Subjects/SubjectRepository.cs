using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Helpers.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Subjects
{
    public class SubjectRepository : GoogleSheetsRepositoryBase, ISubjectRepository
    {
        private string SubjectsRange { get; }
        private IConfiguration Configuration { get; }
        private ILogger<SubjectRepository> Logger { get; }

        public SubjectRepository(
            IConfiguration configuration, ILogger<SubjectRepository> logger)
        : base(configuration, configuration["ScheduleSpreadSheetId"])
        {
            Configuration = configuration;
            Logger = logger;
            SubjectsRange = configuration["SubjectsRange"];
        }
        
        public async Task<Subject[]> Get(Course course)
        {
            var semester = SemesterHelper.Current;
            
            var subjectsRange = await GetRequest(SubjectsRange).ExecuteAsync();
            
            return subjectsRange.Values.Select(subject => new Subject(
                    subject[0] as string ?? throw new ValidationException("Empty subject name"),
                    (Course)ParsingHelper.ParseInt(subject[1]),
                     (Semester)ParsingHelper.ParseInt(subject[2]),
                    ParsingHelper.ParseSubjectCategory(subject[3])))
                .Where(s => s.Course == course && s.Semester == semester)
                .ToArray();
        }
    }
}