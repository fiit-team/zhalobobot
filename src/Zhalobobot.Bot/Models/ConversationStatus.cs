using System.ComponentModel;

namespace Zhalobobot.Bot.Models
{
    public enum ConversationStatus
    {
        [Description("Стандартное состояние.")]
        Default,
        [Description("Ожидание обратной связи от пользователя.")]
        AwaitingFeedback,
        [Description("Ожидание подтвеждения отправить обратную связь.")]
        AwaitingConfirmation
    }
}
