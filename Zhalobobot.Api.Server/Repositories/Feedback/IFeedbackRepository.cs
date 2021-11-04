using System.Threading.Tasks;

namespace Zhalobobot.Api.Server.Repositories.Feedback
{
    public interface IFeedbackRepository
    {
        public Task Add(Zhalobobot.Common.Models.Feedback.Feedback feedback);
    }
}