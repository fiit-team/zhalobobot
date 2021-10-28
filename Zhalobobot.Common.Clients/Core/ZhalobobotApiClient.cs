using Zhalobobot.Common.Clients.Feedback;
using Zhalobobot.Common.Clients.Student;
using Zhalobobot.Common.Clients.Subject;

namespace Zhalobobot.Common.Clients.Core
{
    public class ZhalobobotApiClient : IZhalobobotApiClient
    {
        public ZhalobobotApiClient(string serverUri = "https://localhost:5001")
        {
            Feedback = new FeedbackClient(serverUri);
            Subject = new SubjectClient(serverUri);
            Student = new StudentClient(serverUri);
        }
        
        public IFeedbackClient Feedback { get; }
        public ISubjectClient Subject { get; }
        public IStudentClient Student { get; }
    }
}