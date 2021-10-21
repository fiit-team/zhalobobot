using System.Threading.Tasks;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Api.Server.Repositories
{
    public interface IGoogleSheetsRepository
    {
        public Task AddFeedbackInfo(Feedback feedback);
    }
}