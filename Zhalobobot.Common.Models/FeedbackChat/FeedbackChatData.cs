using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Common.Models.FeedbackChat;

public record FeedbackChatData(
    long ChatId,
    FeedbackType[] FeedbackTypes,
    string[] SubjectNames,
    StudyGroup[] StudyGroups,
    bool IncludeFullStudentInfo);
