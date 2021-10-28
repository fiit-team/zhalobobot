using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Helpers.Extensions;

namespace Zhalobobot.Common.Clients.Feedback
{
    public class FeedbackClient : IFeedbackClient
    {
        private readonly HttpClient client = new();

        private readonly string serverUri;

        public FeedbackClient(string serverUri = "https://localhost:5001")
        {
            this.serverUri = serverUri;
        }
        
        public async Task<ZhalobobotResult> AddFeedback(Models.Feedback.Feedback feedback)
        {
            var content = feedback.SerializeToJsonContent();

            var response = await client.PostAsync($"{serverUri}/feedback", content);

            return new ZhalobobotResult(response.IsSuccessStatusCode, response.StatusCode);
        }
    }
}