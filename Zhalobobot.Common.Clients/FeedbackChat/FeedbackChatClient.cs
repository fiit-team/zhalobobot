using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Common.Clients.FeedbackChat;

public class FeedbackChatClient : ClientBase, IFeedbackChatClient
{
    public FeedbackChatClient(HttpClient client, string serverUri) 
        : base("feedbackChat", client, serverUri)
    {
    }

    public Task<ZhalobobotResult<FeedbackChatDataDto>> GetAll()
        => Method<FeedbackChatDataDto>("getAll").CallAsync();
}