using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Bot.Settings
{
    public class Settings
    {
        public FeedbackChatData[] FeedbackChatSettings { get; init; }

        public string ServerAddress { get; init; }
    }
}
