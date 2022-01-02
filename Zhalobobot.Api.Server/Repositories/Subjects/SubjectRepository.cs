using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Subjects
{
    public class SubjectRepository : GoogleSheetsRepositoryBase, ISubjectRepository
    {
        private string SubjectsRange { get; }

        public SubjectRepository(IConfiguration configuration)
            : base(configuration, configuration["ScheduleSpreadSheetId"])
        {
            SubjectsRange = configuration["SubjectsRange"];
        }

        public async Task<Subject[]> GetAll()
        {
            var semester = SemesterHelper.Current;
            
            var subjectsRange = await GetRequest(SubjectsRange).ExecuteAsync();
            
            return subjectsRange.Values.Select(subject => new Subject(
                    subject[0] as string ?? throw new ValidationException("Empty subject name"),
                    ParsingHelper.ParseEnum<Course>(subject[1]),
                    ParsingHelper.ParseEnum<Semester>(subject[2]),
                    ParsingHelper.ParseSubjectCategory(subject[3])))
                .Where(s => s.Semester == semester)
                .ToArray();
        }
    }
}