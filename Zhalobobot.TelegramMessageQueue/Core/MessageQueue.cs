using System.Collections.Concurrent;
using Telegram.Bot.Types;
using Zhalobobot.TelegramMessageQueue.Models;

namespace Zhalobobot.TelegramMessageQueue.Core;

internal class MessageQueue
{
    private readonly ConcurrentDictionary<MessagePriority, ConcurrentQueue<Task<Message>>> priorityToQueue;

    public MessageQueue()
    {
        priorityToQueue = new ConcurrentDictionary<MessagePriority, ConcurrentQueue<Task<Message>>>();
        foreach (var priority in Enum.GetValues<MessagePriority>())
            priorityToQueue[priority] = new ConcurrentQueue<Task<Message>>();
    }

    public void Enqueue(MessagePriority priority, Task<Message> callback)
        => priorityToQueue[priority].Enqueue(callback);

    public bool TryDequeue(MessagePriority priority, out Task<Message>? message)
        => priorityToQueue[priority].TryDequeue(out message);

    public bool TryPeek(MessagePriority priority, out Task<Message>? message)
        => priorityToQueue[priority].TryPeek(out message);

    public int Count(MessagePriority priority)
        => priorityToQueue[priority].Count;

    public bool IsEmpty(MessagePriority priority)
        => priorityToQueue[priority].IsEmpty;

    public int TotalCount()
        => Enum.GetValues<MessagePriority>()
            .Select(p => priorityToQueue[p].Count)
            .Sum();
}