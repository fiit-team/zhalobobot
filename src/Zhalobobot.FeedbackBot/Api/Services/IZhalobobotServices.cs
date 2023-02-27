namespace Zhalobobot.Bot.Api.Services;

public interface IZhalobobotServices
{
     FeedbackService Feedback { get; }
     SubjectsService Subject { get; }
     StudentsService Student { get; }
     ScheduleService Schedule { get; }
     ReplyService Reply { get; }
     FeedbackChatService FeedbackChat { get; }
}