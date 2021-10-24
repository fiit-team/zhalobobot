namespace Zhalobobot.Common.Models.Feedback
{
    public record Feedback(
        FeedbackType Type,
        Subject Subject,
        string? Message)
    {
        public static Feedback Urgent => new(FeedbackType.UrgentFeedback, new Subject(""), null);
        
        public static Feedback General => new(FeedbackType.GeneralFeedback, new Subject(""), null);

        public static Feedback Subj => new(FeedbackType.SubjectFeedback, new Subject(""), null);
    }
}