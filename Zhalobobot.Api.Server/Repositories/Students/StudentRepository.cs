using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Helpers.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;

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
                student.Course is null ? string.Empty : student.Course,
                student.Group is null ? string.Empty : student.Group,
                student.Subgroup is null ? string.Empty : student.Subgroup,
                student.Name?.LastName ?? string.Empty,
                student.Name?.FirstName ?? string.Empty,
                student.Name?.MiddleName ?? string.Empty,
                student.ChatId
            };

            await AppendRequest(StudentsRange, values).ExecuteAsync();
        }

        public async Task<Student[]> GetAll()
        {
            var values = await GetRequest(StudentsRange).ExecuteAsync();

            return values.Values.Select(student => new Student(
                    student[0] as string ?? throw new ValidationException("Id is empty!"),
                    student[1] as string ?? string.Empty,
                    (Course)ParsingHelper.ParseInt(student[2]),
                    (Group)ParsingHelper.ParseInt(student[3]),
                    (Subgroup)ParsingHelper.ParseInt(student[4]),
                    null, // TODO: вернуть имя вместо null (пока не нужно)
                    student[8] as string ?? throw new ValidationException("ChatId is empty!")))
                .ToArray();
        }

        public async Task<Student[]> GetByCourseAndGroupAndSubgroup(Course course, Group group, Subgroup subgroup)
        {
            var students = await GetAll();
            
            return students
                .Where(s => s.Course == course && s.Group == group && s.Subgroup == subgroup)
                .ToArray();
        }

        public async Task<Student?> GetById(string telegramId)
        {
            var students = await GetRequest(StudentsRange).ExecuteAsync();

            var student = students.Values.FirstOrDefault(v => v[0].ToString() == telegramId);

            if (student == null)
                return null;
                
            var course = ParsingHelper.ParseNullableInt(student[2]);
            var group = ParsingHelper.ParseNullableInt(student[3]);
            var subgroup = ParsingHelper.ParseNullableInt(student[4]);

            return new Student(
                telegramId,
                student[1] as string,
                course is null ? null : (Course) course,
                group is null ? null : (Group) group,
                subgroup is null ? null : (Subgroup) subgroup,
                ParsingHelper.ParseName(student[1], student[2], student[3]),
                (string) student[8]);;
        }
    }
}