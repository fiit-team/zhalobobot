using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Bot.Settings
{
    public class FeedbackChatSettings
    {
        public long ChatId { get; init; }

        public FeedbackType[] FeedbackTypes { get; init; } = new FeedbackType[0];

        public string[] Subjects { get; init; } = new string[0];

        public StudentSettings[] StudentSettings { get; init; } = new StudentSettings[0];

        public bool IncludeFullStudentInfo { get; init; } = false;
    }
}
