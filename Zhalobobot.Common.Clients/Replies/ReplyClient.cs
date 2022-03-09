using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Reply.Requests;

namespace Zhalobobot.Common.Clients.Reply
{
    public class ReplyClient : ClientBase, IReplyClient
    {
        public ReplyClient(HttpClient client, string serverUri)
            : base("reply", client, serverUri)
        {
        }

        public Task<ZhalobobotResult> Add(AddReplyRequest request) 
            => Method("add").CallAsync(request);

        public Task<ZhalobobotResult<Models.Reply.Reply[]>> GetAll()
            => Method<Models.Reply.Reply[]>("getAll").CallAsync();
    }
}