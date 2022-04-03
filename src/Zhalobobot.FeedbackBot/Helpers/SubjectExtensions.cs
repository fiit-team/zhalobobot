using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Helpers;

internal static class SubjectExtensions
{
    public static IEnumerable<Subject> FilterFor(this IEnumerable<Subject> subjects, Course course)
        => subjects.Where(s => s.Course == course && s.Semester == SemesterHelper.Current);

    public static IEnumerable<Subject> FilterFor(this IEnumerable<Subject> subjects, Student student)
        => subjects.FilterFor(student.Course);
}