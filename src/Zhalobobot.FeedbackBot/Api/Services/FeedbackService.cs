using System.Threading.Tasks;
using Zhalobobot.Bot.Api.Repositories.Feedback;
using Zhalobobot.Common.Models.Feedback.Requests;

namespace Zhalobobot.Bot.Api.Services;

public class FeedbackService
{
    private readonly IFeedbackRepository repository;

    public FeedbackService(IFeedbackRepository repository)
    {
        this.repository = repository;
    }

    public async Task Add(AddFeedbackRequest request)
        => await repository.Add(request.Feedback);
}