namespace Zhalobobot.Common.Models.Reply
{
    public class Reply
    {
        public long UserId { get; }

        public string Username { get; }

        public long ChatId { get; }

        public int MessageId { get; }

        public string Message { get; }

        public long ChildChatId { get; }

        public int ChildMessageId { get; }

        public Reply ParentReply { get; }

        public Reply(long userId, string username, long chatId, int messageId, string message, long childChatId, int childMessageId, Reply reply = null)
        {
            this.UserId = userId;
            this.Username = username;
            this.ChatId = chatId;
            this.MessageId = messageId;
            this.Message = message;
            this.ChildChatId = childChatId;
            this.ChildMessageId = childMessageId;
            this.ParentReply = reply;
        }
    }
}