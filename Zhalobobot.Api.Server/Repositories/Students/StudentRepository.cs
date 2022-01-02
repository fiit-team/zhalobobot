using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Api.Server.Repositories.Students
{
    public class StudentRepository : GoogleSheetsRepositoryBase, IStudentRepository
    {
        private string StudentsRange { get; }

        public StudentRepository(IConfiguration configuration) 
            : base(configuration, configuration["FeedbackSpreadSheetId"])
        {
            StudentsRange = configuration["StudentsRange"];
        }

        public async Task Add(Student student)
        {
            var values = new List<object>
            {
                student.Id,
                student.Username ?? string.Empty,
                student.Course,
                student.Group,
                student.Subgroup,
                student.Name?.LastName ?? string.Empty,
                student.Name?.FirstName ?? string.Empty,
                student.Name?.MiddleName ?? string.Empty
            };

            await AppendRequest(StudentsRange, values).ExecuteAsync();
        }

        public async Task<Student[]> GetAll()
        {
            var values = await GetRequest(StudentsRange).ExecuteAsync();

            return values.Values.Select(student => new Student(
                ParsingHelper.ParseLong(student[0]),
                student[1] as string ?? string.Empty,
                ParsingHelper.ParseEnum<Course>(student[2]),
                ParsingHelper.ParseEnum<Group>(student[3]),
                ParsingHelper.ParseEnum<Subgroup>(student[4]),
                new Name(student[5] as string ?? string.Empty, student[6] as string ?? string.Empty, null)))
                .ToArray();
        }
    }
}