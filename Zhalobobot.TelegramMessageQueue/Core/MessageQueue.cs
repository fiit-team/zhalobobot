using System.Collections.Concurrent;
using Telegram.Bot.Types;
using Zhalobobot.TelegramMessageQueue.Models;

namespace Zhalobobot.TelegramMessageQueue.Core;

internal class MessageQueue
{
    private readonly ConcurrentDictionary<MessagePriority, ConcurrentQueue<QueueItem>> priorityToQueue;

    public MessageQueue()
    {
        priorityToQueue = new ConcurrentDictionary<MessagePriority, ConcurrentQueue<QueueItem>>();
        foreach (var priority in Enum.GetValues<MessagePriority>())
            priorityToQueue[priority] = new ConcurrentQueue<QueueItem>();
    }

    public void Enqueue(QueueItem item)
        => priorityToQueue[item.Priority].Enqueue(item);

    public bool TryDequeue(MessagePriority priority, out QueueItem? item)
        => priorityToQueue[priority].TryDequeue(out item);

    public bool TryPeek(MessagePriority priority, out QueueItem? item)
        => priorityToQueue[priority].TryPeek(out item);

    public int Count(MessagePriority priority)
        => priorityToQueue[priority].Count;

    public bool IsEmpty(MessagePriority priority)
        => priorityToQueue[priority].IsEmpty;

    public int TotalCount()
        => Enum.GetValues<MessagePriority>()
            .Select(p => priorityToQueue[p].Count)
            .Sum();
}