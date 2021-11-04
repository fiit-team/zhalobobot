using System.Threading.Tasks;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Subjects
{
    public interface ISubjectRepository
    {
        Task<Subject[]> Get(int course);
    }
}