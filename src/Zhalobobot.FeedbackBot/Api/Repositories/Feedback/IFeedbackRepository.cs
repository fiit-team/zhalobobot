using System.Threading.Tasks;

namespace Zhalobobot.Bot.Api.Repositories.Feedback;

public interface IFeedbackRepository
{
    public Task Add(Zhalobobot.Common.Models.Feedback.Feedback feedback);
}