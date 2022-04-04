using Zhalobobot.Common.Models.FeedbackChat;

namespace Zhalobobot.Bot.Cache;

public class FeedbackChatDataCache : EntityCacheBase<FeedbackChatData>
{
    public FeedbackChatDataCache(FeedbackChatData[] entities) 
        : base(entities)
    {
    }
}