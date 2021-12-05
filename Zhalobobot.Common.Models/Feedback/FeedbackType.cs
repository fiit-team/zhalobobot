using System.ComponentModel;

namespace Zhalobobot.Common.Models.Feedback
{
    public enum FeedbackType
    {
        [Description("Срочная обратная связь.")]
        Urgent,
        [Description("Общая обратная связь.")]
        General,
        [Description("Обратная связь по предмету.")]
        Subject
    }
}