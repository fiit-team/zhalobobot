using System.Collections.Concurrent;

namespace Zhalobobot.TelegramMessageQueue.Core;

public class MessageQueue
{
    private readonly Dictionary<MessagePriority, ConcurrentQueue<QueueItem>> priorityToQueue;

    public MessageQueue()
    {
        priorityToQueue = new Dictionary<MessagePriority, ConcurrentQueue<QueueItem>>();
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