using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Common.Models.Feedback
{
    public record Feedback(
        FeedbackType Type,
        OldSubject? Subject = null,
        string? Message = null,
        AbTestStudent? Student = null,
        SubjectSurvey? SubjectSurvey = null)
    {
        public static Feedback Urgent => new(FeedbackType.UrgentFeedback);
        
        public static Feedback General => new(FeedbackType.GeneralFeedback);

        public static Feedback Subj => new(FeedbackType.SubjectFeedback);
    }
}