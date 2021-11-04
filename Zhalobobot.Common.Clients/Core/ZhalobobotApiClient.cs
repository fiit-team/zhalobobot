using System.Net.Http;
using Zhalobobot.Common.Clients.Feedback;
using Zhalobobot.Common.Clients.Student;
using Zhalobobot.Common.Clients.Subject;

namespace Zhalobobot.Common.Clients.Core
{
    public class ZhalobobotApiClient : IZhalobobotApiClient
    {
        private readonly HttpClient client = new();

        public ZhalobobotApiClient(string serverUri = "https://localhost:5001")
        {
            Feedback = new FeedbackClient(client, serverUri);
            Subject = new SubjectClient(client, serverUri);
            Student = new StudentClient(client, serverUri);
        }
        
        public IFeedbackClient Feedback { get; }
        public ISubjectClient Subject { get; }
        public IStudentClient Student { get; }
    }
}