namespace Zhalobobot.Common.Models.Subject
{
    public record Subject(
        string Name,
        int Course,
        int Semester,
        SubjectCategory Category);//,
        // EventTime[] Events); // когда будет проводиться в течение этой недели
}