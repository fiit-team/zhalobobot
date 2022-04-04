using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.FeedbackChat;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Api.Server.Controllers;

[Route("feedbackChat")]
public class FeedbackChatController
{
    private readonly IFeedbackChatRepository repository;

    public FeedbackChatController(IFeedbackChatRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost("getAll")]
    public async Task<FeedbackChatDataDto> GetAll()
    {
        var (shouldBeUpdated, feedbackChatData) = await repository.GetAll();
        return new(shouldBeUpdated, feedbackChatData.ToArray());
    }
}