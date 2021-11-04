using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Feedback;
using Zhalobobot.Common.Models.Feedback.Requests;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("feedback")]
    public class FeedbackController
    {
        private readonly IFeedbackRepository repository;

        public FeedbackController(IFeedbackRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("add")]
        public async Task Add([FromBody] AddFeedbackRequest request)
            => await repository.Add(request.Feedback);
    }
}