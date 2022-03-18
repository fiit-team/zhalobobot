using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Bot.Settings
{
    public class FeedbackChatSettings
    {
        public long ChatId { get; init; }

        public FeedbackType[] FeedbackTypes { get; init; } = new FeedbackType[0];

        public int[] Subjects { get; init; } = new int[0];

        public StudentSettings[] StudentSettings { get; init; } = new StudentSettings[0];

        public bool IncludeStudentInfo { get; init; } = false;
    }
}
