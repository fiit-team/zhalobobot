using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback.Requests;

namespace Zhalobobot.Common.Clients.Feedback
{
    public interface IFeedbackClient
    {
        Task<ZhalobobotResult> AddFeedback(AddFeedbackRequest request);
    }
}