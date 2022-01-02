using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Common.Clients.Subject
{
    public class SubjectClient : ClientBase, ISubjectClient
    {
        public SubjectClient(HttpClient client, string serverUri)
            : base("subjects", client, serverUri)
        {
        }

        public Task<ZhalobobotResult<Models.Subject.Subject[]>> GetAll()
            => Method<Models.Subject.Subject[]>("getAll").CallAsync();
    }
}