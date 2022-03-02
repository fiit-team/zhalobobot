using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject
{
    public record Subject(
        string Name,
        Course Course,
        Semester Semester,
        SubjectCategory Category,
        int StudentsToNotifyPercent);
}