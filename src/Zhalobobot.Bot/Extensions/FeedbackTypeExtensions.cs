using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Extensions
{
    public static class FeedbackTypeExtensions
    {
        public static string GetString(this FeedbackType feedbackType)
        {
            return feedbackType switch
            {
                FeedbackType.GeneralFeedback => "Общая",
                FeedbackType.SubjectFeedback => "Предмет",
                FeedbackType.UrgentFeedback => "Срочная",
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
