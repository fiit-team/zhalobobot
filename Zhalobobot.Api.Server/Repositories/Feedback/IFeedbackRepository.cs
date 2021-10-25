using System.Threading.Tasks;

namespace Zhalobobot.Api.Server.Repositories.Feedback
{
    public interface IFeedbackRepository
    {
        public Task AddFeedback(Zhalobobot.Common.Models.Feedback.Feedback feedback);
    }
}