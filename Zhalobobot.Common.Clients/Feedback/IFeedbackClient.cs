using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Common.Clients.Feedback
{
    public interface IFeedbackClient
    {
        Task<ZhalobobotResult> AddFeedback(Models.Feedback.Feedback feedback);
    }
}