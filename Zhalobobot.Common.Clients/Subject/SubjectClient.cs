using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Common.Clients.Subject
{
    public class SubjectClient : ISubjectClient
    {
        private readonly HttpClient client = new();

        private readonly string serverUri;

        public SubjectClient(string serverUri = "https://localhost:5001")
        {
            this.serverUri = serverUri;
        }
        
        public async Task<ZhalobobotResult<Models.Feedback.Subject[]>> GetSubjects()
        {
            var response = await client.GetAsync($"{serverUri}/subjects");

            if (!response.IsSuccessStatusCode)
                return new ZhalobobotResult<Models.Feedback.Subject[]>(
                    Array.Empty<Models.Feedback.Subject>(),
                    response.IsSuccessStatusCode, 
                    response.StatusCode);
            
            var subjects = (await response.Content.ReadAsStreamAsync()).FromJsonStream<Models.Feedback.Subject[]>();

            return new ZhalobobotResult<Models.Feedback.Subject[]>(subjects, response.IsSuccessStatusCode, response.StatusCode);
        }
    }
}