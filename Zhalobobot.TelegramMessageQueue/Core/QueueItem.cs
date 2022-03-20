namespace Zhalobobot.TelegramMessageQueue.Core;

public record QueueItem(MessagePriority Priority, Func<Task> TaskGenerator, long? GroupChatId);