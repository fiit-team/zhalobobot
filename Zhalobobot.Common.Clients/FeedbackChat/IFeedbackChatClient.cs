using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Common.Clients.FeedbackChat;

public interface IFeedbackChatClient
{
    public Task<ZhalobobotResult<FeedbackChatDataDto>> GetAll();
}