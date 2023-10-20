using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Api.Repositories.Common;
using Zhalobobot.Bot.Settings;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Api.Repositories.Students;

[RequiresSecretConfiguration(typeof(BotSecrets))]
public class StudentRepository : GoogleSheetsRepositoryBase, IStudentRepository
{
    private string StudentsRange { get; }

    public StudentRepository(IVostokHostingEnvironment environment) 
        : base(environment, environment.SecretConfigurationProvider.Get<BotSecrets>().FeedbackSpreadSheetId)
    {
        StudentsRange = environment.SecretConfigurationProvider.Get<BotSecrets>().StudentsRange;
    }

    public async Task Add(Student student)
    {
        var values = GetValuesToWrite(student);

        await AppendRequest(StudentsRange, values).ExecuteAsync();
    }

    public async Task Update(Student student)
    {
        var values = GetValuesToWrite(student);

        var students = await GetAll();
        if (students.Any(s => s.Id == student.Id))
        {
            var index = Array.FindIndex(students.Select(s => s.Id).ToArray(), s => s == student.Id);
            var listName = StudentsRange.Split("!")[0];
            await UpdateRequest($"{listName}!A{2 + index}:Z{2 + index}", values).ExecuteAsync();
        }
        else
        {
            await AppendRequest(StudentsRange, values).ExecuteAsync();
        }
    }

    public async Task<Student[]> GetAll()
    {
        var values = await GetRequest(StudentsRange).ExecuteAsync();

        return values.Values.Select(student =>
            {
                object? middleName = null;
                object? specialCourses = null;
                if (student.Count > 7)
                    middleName = student[7];
                if (student.Count > 8)
                    specialCourses = student[8];
                    
                return new Student(
                    ParsingHelper.ParseLong(student[0]),
                    student[1] as string ?? string.Empty,
                    ParsingHelper.ParseEnum<Course>(student[2]),
                    ParsingHelper.ParseEnum<Group>(student[3]),
                    ParsingHelper.ParseEnum<Subgroup>(student[4]),
                    ParsingHelper.ParseName(student[5], student[6], middleName),
                    ParsingHelper.ParseSpecialCourseNames(specialCourses));
            })
            .ToArray();
    }

    private static List<object> GetValuesToWrite(Student student)
        => new()
        {
            student.Id,
            student.Username ?? string.Empty,
            student.Course,
            student.Group,
            student.Subgroup,
            student.Name?.LastName ?? string.Empty,
            student.Name?.FirstName ?? string.Empty,
            student.Name?.MiddleName ?? string.Empty,
            string.Join(";", student.SpecialCourseNames)
        };
}