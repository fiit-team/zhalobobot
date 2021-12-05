using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Common.Models.Helpers
{
    public static class FeedbackTypeExtensions
    {
        public static string GetString(this FeedbackType feedbackType)
        {
            return feedbackType switch
            {
                FeedbackType.General => "Общая",
                FeedbackType.Subject => "Предмет",
                FeedbackType.Urgent => "Срочная",
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
