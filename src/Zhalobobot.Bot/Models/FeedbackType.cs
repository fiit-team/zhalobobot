using System.ComponentModel;

namespace Zhalobobot.Bot.Models
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
