using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Common.Models.Feedback
{
    public record Feedback(
        FeedbackType Type,
        Subject Subject,
        string? Message,
        AbTestStudent? Student)
    {
        public static Feedback Urgent => new(FeedbackType.UrgentFeedback, new Subject(""), null, null);
        
        public static Feedback General => new(FeedbackType.GeneralFeedback, new Subject(""), null, null);

        public static Feedback Subj => new(FeedbackType.SubjectFeedback, new Subject(""), null, null);
    }
}