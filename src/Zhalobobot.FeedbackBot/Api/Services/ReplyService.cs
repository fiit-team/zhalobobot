using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Bot.Api.Repositories.Replies;
using Zhalobobot.Common.Models.Reply;
using Zhalobobot.Common.Models.Reply.Requests;

namespace Zhalobobot.Bot.Api.Services;

public class ReplyService
{
    private readonly IReplyRepository repository;

    public ReplyService(IReplyRepository repository)
    {
        this.repository = repository;
    }

    public async Task Add([FromBody] AddReplyRequest request)
        => await repository.Add(request.Reply);

    public async Task<Reply[]> GetAll()
        => await repository.GetAll();
}