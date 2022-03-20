namespace Zhalobobot.TelegramMessageQueue.Core;

public static class MessageQueueExtensions
{
    public static IEnumerable<QueueItem> GetItemsToExecute(this MessageQueue queue, int allowedCount)
    {
        foreach (var priority in Enum.GetValues<MessagePriority>())
        {
            while (!queue.IsEmpty(priority))
            {
                if (allowedCount <= 0)
                    yield break;
                if (!queue.TryDequeue(priority, out var itemToExecute) || itemToExecute == null)
                    continue;

                allowedCount -= 1;
                yield return itemToExecute;
            }
        }
    }
}