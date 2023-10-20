using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Bot.Settings
{
    public class Settings
    {
        public string ServerAddress { get; set; } = null!;
        public string UrgentFeedbackChatId { get; set; } = null!;
        public string DesignFeedbackChatId { get; set; } = null!;
    }
}
