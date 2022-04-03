using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record Student(
        long Id,
        string? Username,
        Course Course,
        Group Group,
        Subgroup Subgroup,
        Name? Name,
        string[] SpecialCourseNames);
}