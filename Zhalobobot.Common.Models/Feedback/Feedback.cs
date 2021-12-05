using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Common.Models.Feedback
{
    public record Feedback(
        FeedbackType Type,
        Student.Student Student,
        string? Message = null,
        Subject.Subject? Subject = null,
        SubjectSurvey? SubjectSurvey = null);
}