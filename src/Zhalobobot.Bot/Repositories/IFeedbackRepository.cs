using System.Threading.Tasks;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Repositories
{
    public interface IFeedbackRepository
    {
        public Task AddFeedbackInfoAsync(FeedbackInfo feedbackInfo);
    }
}
