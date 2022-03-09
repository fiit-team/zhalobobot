namespace Zhalobobot.Common.Models.Reply
{
    public record Reply(
        long UserId,
        string Username,
        long ChatId,
        int MessageId,
        string Message,
        long ChildChatId,
        int ChildMessageId);
}