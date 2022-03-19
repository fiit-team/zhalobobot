using Telegram.Bot.Types;

namespace Zhalobobot.TelegramMessageQueue.Core;

public record QueueItem(MessagePriority Priority, Func<Task<Message>> TaskGenerator, long? GroupChatId);