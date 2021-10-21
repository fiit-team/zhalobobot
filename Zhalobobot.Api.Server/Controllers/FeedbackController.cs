using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("feedback")]
    public class FeedbackController
    {
        private readonly IGoogleSheetsRepository repository;

        public FeedbackController(IGoogleSheetsRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task Add([FromBody] Feedback feedback)
        {
            await repository.AddFeedbackInfo(feedback);
        }
    }
}