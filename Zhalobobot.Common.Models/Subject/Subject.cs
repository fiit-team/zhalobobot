using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject
{
    public record Subject(
        string Name,
        Course Course,
        Semester Semester,
        SubjectCategory Category);//,
    // EventTime[] Events); // когда будет проводиться в течение этой недели
}