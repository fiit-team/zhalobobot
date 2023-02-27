using System.Linq;
using System.Threading.Tasks;
using Zhalobobot.Bot.Api.Repositories.FeedbackChat;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Bot.Api.Services;

public class FeedbackChatService
{
    private readonly IFeedbackChatRepository repository;

    public FeedbackChatService(IFeedbackChatRepository repository)
    {
        this.repository = repository;
    }

    public async Task<FeedbackChatDataDto> GetAll()
    {
        var (shouldBeUpdated, feedbackChatData) = await repository.GetAll();
        return new(shouldBeUpdated, feedbackChatData.ToArray());
    }
}