using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Common.Models.Feedback
{
    public record Feedback(
        FeedbackType Type,
        Subject.Subject Subject,
        string? Message = null,
        AbTestStudent? Student = null,
        SubjectSurvey? SubjectSurvey = null)
    {
        public static Feedback Urgent => new(FeedbackType.UrgentFeedback, new Subject.Subject("", null));
        
        public static Feedback General => new(FeedbackType.GeneralFeedback, new Subject.Subject("", null));

        public static Feedback Subj => new(FeedbackType.SubjectFeedback, new Subject.Subject("", null));
    }
}