using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Feedback;
using Zhalobobot.Common.Models.Reply;
using Zhalobobot.Common.Models.Reply.Requests;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("reply")]
    public class ReplyController
    {
        private readonly IReplyRepository repository;

        public ReplyController(IReplyRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("add")]
        public async Task Add([FromBody] AddReplyRequest request)
            => await repository.Add(request.Reply);

        [HttpPost("getAll")]
        public async Task<Reply[]> GetAll()
            => await repository.GetAll();
    }
}