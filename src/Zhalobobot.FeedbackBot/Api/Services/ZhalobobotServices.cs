using Zhalobobot.Bot.Api.Repositories.Feedback;
using Zhalobobot.Bot.Api.Repositories.FeedbackChat;
using Zhalobobot.Bot.Api.Repositories.FiitStudentsData;
using Zhalobobot.Bot.Api.Repositories.Replies;
// using Zhalobobot.Bot.Api.Repositories.Schedule;
using Zhalobobot.Bot.Api.Repositories.Students;
using Zhalobobot.Bot.Api.Repositories.Subjects;

namespace Zhalobobot.Bot.Api.Services;

public class ZhalobobotServices : IZhalobobotServices
{
    public ZhalobobotServices(
        IFeedbackRepository feedback,
        ISubjectRepository subject,
        IStudentRepository student,
        // IScheduleRepository schedule,
        IReplyRepository reply,
        IFeedbackChatRepository feedbackChat,
        IFiitStudentsDataRepository fiitStudentsDataRepository)
    {
        Feedback = new FeedbackService(feedback);
        Subject = new SubjectsService(subject);
        Student = new StudentsService(student, fiitStudentsDataRepository);
        // Schedule = new ScheduleService(schedule);
        Reply = new ReplyService(reply);
        FeedbackChat = new FeedbackChatService(feedbackChat);
    }
        
    public FeedbackService Feedback { get; }
    public SubjectsService Subject { get; }
    public StudentsService Student { get; }
    // public ScheduleService Schedule { get; }
    public ReplyService Reply { get; }
    public FeedbackChatService FeedbackChat { get; }
}