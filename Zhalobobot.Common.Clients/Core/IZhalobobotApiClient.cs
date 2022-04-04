using Zhalobobot.Common.Clients.Feedback;
using Zhalobobot.Common.Clients.FeedbackChat;
using Zhalobobot.Common.Clients.Reply;
using Zhalobobot.Common.Clients.Schedule;
using Zhalobobot.Common.Clients.Student;
using Zhalobobot.Common.Clients.Subject;

namespace Zhalobobot.Common.Clients.Core
{
    public interface IZhalobobotApiClient
    {
        IFeedbackClient Feedback { get; }
        ISubjectClient Subject { get; }
        IStudentClient Student { get; }
        IScheduleClient Schedule { get; }
        IReplyClient Reply { get; }
        IFeedbackChatClient FeedbackChat { get; }
    }
}