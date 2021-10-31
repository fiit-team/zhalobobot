using System.ComponentModel;

namespace Zhalobobot.Bot.Models
{
    public enum ConversationStatus
    {
        [Description("Стандартное состояние.")]
        Default,
        AwaitingRating,
        AwaitingLikedPointsPollAnswer,
        AwaitingUnlikedPointsPollAnswer,
        [Description("Ожидание обратной связи от пользователя.")]
        AwaitingMessage,
        [Description("Ожидание подтвеждения отправить обратную связь.")]
        AwaitingConfirmation
    }
}
