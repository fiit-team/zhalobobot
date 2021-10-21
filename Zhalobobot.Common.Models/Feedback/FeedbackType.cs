using System.ComponentModel;

namespace Zhalobobot.Common.Models.Feedback
{
    public enum FeedbackType
    {
        [Description("Срочная обратная связь.")]
        UrgentFeedback,
        [Description("Общая обратная связь.")]
        GeneralFeedback,
        [Description("Обратная связь по предмету.")]
        SubjectFeedback
    }
}