// using System.Net.Http;
// using Zhalobobot.Common.Clients.Feedback;
// using Zhalobobot.Common.Clients.FeedbackChat;
// using Zhalobobot.Common.Clients.Reply;
// using Zhalobobot.Common.Clients.Schedule;
// using Zhalobobot.Common.Clients.Student;
// using Zhalobobot.Common.Clients.Subject;
//
// namespace Zhalobobot.Common.Clients.Core
// {
//     public class ZhalobobotApiClient : IZhalobobotApiClient
//     {
//         private readonly HttpClient client = new();
//
//         public ZhalobobotApiClient(string serverUri = "https://localhost:5001")
//         {
//             Feedback = new FeedbackClient(client, serverUri);
//             Subject = new SubjectClient(client, serverUri);
//             Student = new StudentClient(client, serverUri);
//             Schedule = new ScheduleClient(client, serverUri);
//             Reply = new ReplyClient(client, serverUri);
//             FeedbackChat = new FeedbackChatClient(client, serverUri);
//         }
//         
//         public IFeedbackClient Feedback { get; }
//         public ISubjectClient Subject { get; }
//         public IStudentClient Student { get; }
//         public IScheduleClient Schedule { get; }
//         public IReplyClient Reply { get; }
//         public IFeedbackChatClient FeedbackChat { get; }
//     }
// }