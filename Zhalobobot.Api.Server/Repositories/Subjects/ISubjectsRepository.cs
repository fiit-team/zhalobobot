using System.Threading.Tasks;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Api.Server.Repositories.Subjects
{
    public interface ISubjectsRepository
    {
        Task<Subject[]> Get();
    }
}