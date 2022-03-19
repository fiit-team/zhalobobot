using Telegram.Bot.Types;
using Zhalobobot.TelegramMessageQueue.Models;

namespace Zhalobobot.TelegramMessageQueue.Core;

public record QueueItem(MessagePriority Priority, Func<Task<Message>> TaskGenerator, long? GroupChatId);