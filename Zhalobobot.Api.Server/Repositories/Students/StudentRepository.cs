using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Helpers.Helpers;
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
                (Course)ParsingHelper.ParseInt(student[2]),
                (Group)ParsingHelper.ParseInt(student[3]),
                (Subgroup)ParsingHelper.ParseInt(student[4]),
                new Name(student[5] as string ?? string.Empty, student[6] as string ?? string.Empty, null)))
                .ToArray();
        }

        public async Task<Student[]> GetByCourseAndGroupAndSubgroup(Course course, Group group, Subgroup subgroup)
        {
            var students = await GetAll();
            
            return students
                .Where(s => s.Course == course && s.Group == group && s.Subgroup == subgroup)
                .ToArray();
        }

        public async Task<Student?> FindById(long telegramId)
        {
            var students = await GetRequest(StudentsRange).ExecuteAsync();

            var student = students.Values?.FirstOrDefault(v => ParsingHelper.ParseLong(v[0]) == telegramId);

            if (student == null)
                return null;
                
            var course = ParsingHelper.ParseInt(student[2]);
            var group = ParsingHelper.ParseInt(student[3]);
            var subgroup = ParsingHelper.ParseInt(student[4]);

            return new Student(
                telegramId,
                student[1] as string,
                (Course) course,
                (Group) group,
                (Subgroup) subgroup,
                ParsingHelper.ParseName(student[1], student[2], student[3]));
        }
    }
}