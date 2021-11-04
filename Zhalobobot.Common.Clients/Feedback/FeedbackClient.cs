using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback.Requests;

namespace Zhalobobot.Common.Clients.Feedback
{
    public class FeedbackClient : ClientBase, IFeedbackClient
    {
        public FeedbackClient(HttpClient client, string serverUri)
            : base("feedback", client, serverUri)
        {
        }

        public Task<ZhalobobotResult> AddFeedback(AddFeedbackRequest request) 
            => Method("add").CallAsync(request);
    }
}