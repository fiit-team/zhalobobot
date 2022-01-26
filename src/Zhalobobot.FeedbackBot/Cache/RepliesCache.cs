using System.Collections.Generic;
using System.Linq;
using Zhalobobot.Common.Models.Reply;

namespace Zhalobobot.Bot.Cache
{
    public class RepliesCache : EntityCacheBase<Reply>
    {
        private readonly List<Reply> Replies = new();

        public RepliesCache(Reply[] replies) 
            : base(replies)
        {
            this.Replies = replies.ToList();
        }

        public Reply? FindBySentMessage(long chatId, int messageId)
        {
            return Replies.FirstOrDefault(x => x.ChildChatId == chatId && x.ChildMessageId == messageId);
        }

        public void Add(Reply reply)
        {
            Replies.Add(reply);
        }
    }
}