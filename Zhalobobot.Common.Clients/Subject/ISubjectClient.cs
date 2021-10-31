using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Common.Clients.Subject
{
    public interface ISubjectClient
    {
        Task<ZhalobobotResult<Models.Subject.Subject[]>> GetSubjects();
    }
}