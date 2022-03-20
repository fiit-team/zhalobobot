using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Zhalobobot.TelegramMessageQueue.Core;

namespace Zhalobobot.Tests.TelegramMessageQueue;

#pragma warning disable 8618

[TestFixture]
public class MessageQueueExtensionsTests
{
    private MessageQueue messageQueue;
    private QueueItem highestPriorityQueueItem;
    private QueueItem highPriorityQueueItem;
    private QueueItem normalPriorityQueueItem;
    private QueueItem lowPriorityQueueItem;

    [SetUp]
    public void SetUp()
    {
        messageQueue = new MessageQueue();
        highestPriorityQueueItem = new QueueItem(MessagePriority.Highest, () => Task.CompletedTask, null);
        highPriorityQueueItem = new QueueItem(MessagePriority.Highest, () => Task.CompletedTask, null);
        normalPriorityQueueItem = new QueueItem(MessagePriority.Normal, () => Task.CompletedTask, null);
        lowPriorityQueueItem = new QueueItem(MessagePriority.Low, () => Task.CompletedTask, null);
        messageQueue.Enqueue(lowPriorityQueueItem);
        messageQueue.Enqueue(highestPriorityQueueItem);
        messageQueue.Enqueue(normalPriorityQueueItem);
        messageQueue.Enqueue(highPriorityQueueItem);
    }

    [Test]
    public void Should_return_items_in_sorted_order()
    {
        messageQueue.GetItemsToExecute(4)
            .Should().BeEquivalentTo(new[]
            {
                highestPriorityQueueItem,
                highPriorityQueueItem,
                normalPriorityQueueItem,
                lowPriorityQueueItem
            });
    }

    [Test]
    public void Should_not_return_more_items_than_contains_in_queue()
    {
        messageQueue.GetItemsToExecute(100)
            .Should().BeEquivalentTo(new[]
            {
                highestPriorityQueueItem,
                highPriorityQueueItem,
                normalPriorityQueueItem,
                lowPriorityQueueItem
            });
    }
    
    [Test]
    public void Should_return_requested_amount_even_if_queue_have_more_items()
    {
        messageQueue.GetItemsToExecute(2)
            .Should().BeEquivalentTo(new[]
            {
                highestPriorityQueueItem,
                highPriorityQueueItem
            });
    }
}