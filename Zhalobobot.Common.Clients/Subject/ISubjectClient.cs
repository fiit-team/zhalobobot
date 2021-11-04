using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Subject.Requests;

namespace Zhalobobot.Common.Clients.Subject
{
    public interface ISubjectClient
    {
        Task<ZhalobobotResult<Models.Subject.Subject[]>> Get(GetSubjectsRequest request);
    }
}