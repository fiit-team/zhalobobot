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

        public async Task<Subject[]> Get(Course? course = null, SubjectCategory? category = null)
        {
            var semester = SemesterHelper.Current;
            
            var subjectsRange = await GetRequest(SubjectsRange).ExecuteAsync();

            var result = subjectsRange.Values.Select(subject => new Subject(
                    subject[0] as string ?? throw new ValidationException("Empty subject name"),
                    (Course)ParsingHelper.ParseInt(subject[1]),
                    (Semester)ParsingHelper.ParseInt(subject[2]),
                    ParsingHelper.ParseSubjectCategory(subject[3])))
                .Where(s => s.Semester == semester);

            if (course.HasValue)
            {
                result = result.Where(s => Equals(s.Course, course));
            }
            if (category.HasValue)
            {
                result = result.Where(s => Equals(s.Category, category));
            }

            return result.ToArray();
        }
    }
}