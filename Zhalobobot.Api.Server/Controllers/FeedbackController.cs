using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Feedback;
using Zhalobobot.Common.Models.Feedback;

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

        [HttpPost]
        [Consumes("application/json")]
        public async Task Add([FromBody] Feedback feedback)
            => await repository.AddFeedback(feedback);
    }
}