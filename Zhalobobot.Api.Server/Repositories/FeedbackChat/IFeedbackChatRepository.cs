using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Api.Server.Repositories.FeedbackChat;

public interface IFeedbackChatRepository
{
    public Task<(bool ShouldBeUpdated, IEnumerable<FeedbackChatData>)> GetAll();
}