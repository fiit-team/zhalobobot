using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Common.Models.Feedback
{
    public record Feedback(
        FeedbackType Type,
        Subject Subject,
        string? Message = null,
        AbTestStudent? Student = null,
        SubjectSurvey? SubjectSurvey = null)
    {
        public static Feedback Urgent => new(FeedbackType.UrgentFeedback, new Subject(""));
        
        public static Feedback General => new(FeedbackType.GeneralFeedback, new Subject(""));

        public static Feedback Subj => new(FeedbackType.SubjectFeedback, new Subject(""));
    }
}