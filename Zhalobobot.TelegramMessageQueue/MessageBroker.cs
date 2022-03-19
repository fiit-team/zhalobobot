using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Vostok.Commons.Time;
using Zhalobobot.Common.Models.Serialization;
using Zhalobobot.TelegramMessageQueue.Core;
using Zhalobobot.TelegramMessageQueue.Models;
using Zhalobobot.TelegramMessageQueue.Settings;

namespace Zhalobobot.TelegramMessageQueue;

public class MessageBroker : IDisposable
{
    private readonly MessageQueue userMessageQueue;
    private readonly ConcurrentDictionary<long, (MessageQueue Queue, int SendCountPerMinute)> groupIdToQueue;
    private readonly PeriodicalAction clearSendCountRoutine;
    private readonly AsyncPeriodicalAction executeUserMessagesRoutine;
    private readonly AsyncPeriodicalAction executeGroupMessagesRoutine;
    private readonly ILogger<MessageBroker> log;

    private MessageBroker(ILogger<MessageBroker> log)
    {
        this.log = log;
        userMessageQueue = new MessageQueue();
        groupIdToQueue = new ConcurrentDictionary<long, (MessageQueue Queue, int SendCountPerMinute)>();
        clearSendCountRoutine = new PeriodicalAction(ClearSendCountPerMinute, LogError, () => TimeSpan.FromMinutes(1));
        executeUserMessagesRoutine = new AsyncPeriodicalAction(ExecuteUserMessages, LogError, () => TimeSpan.FromSeconds(1));
        executeGroupMessagesRoutine = new AsyncPeriodicalAction(ExecuteGroupMessages, LogError, () => TimeSpan.FromSeconds(1));
        
        clearSendCountRoutine.Start();
        executeUserMessagesRoutine.Start();
        executeGroupMessagesRoutine.Start();
    }

    public void SendToUser(MessagePriority priority, Func<Task<Message>> taskGenerator)
        => userMessageQueue.Enqueue(new QueueItem(priority, taskGenerator, null));

    public void SendToGroupChat(long groupId, MessagePriority priority, Func<Task<Message>> taskGenerator)
    {
        if (!groupIdToQueue.ContainsKey(groupId))
            groupIdToQueue.TryAdd(groupId, (new MessageQueue(), 0));
        
        groupIdToQueue[groupId].Queue.Enqueue(new QueueItem(priority, taskGenerator, groupId));
    }

    public void Dispose()
    {
        clearSendCountRoutine.Stop();
        executeUserMessagesRoutine.Stop();
        executeGroupMessagesRoutine.Stop();
    }

    #region PeriodicalActionMethods

    private void ClearSendCountPerMinute()
    {
        foreach (var (groupId, (queue, sendCountPerMinute)) in groupIdToQueue)
        {
            if (sendCountPerMinute == 0)
                continue;
            if (groupIdToQueue.TryUpdate(groupId, (queue, 0), (queue, sendCountPerMinute))) 
                continue;
                
            while (true)
            {
                var foundValue = groupIdToQueue.TryGetValue(groupId, out var oldResult);
                if (!foundValue || oldResult.SendCountPerMinute == 0)
                    break;
                if (groupIdToQueue.TryUpdate(groupId, (oldResult.Queue, 0), oldResult))
                    break;
            }
        }
    }
    
    private void LogError(Exception e) => log.LogError(e.Message);
    
    private async Task ExecuteUserMessages() 
        => await Task.WhenAll(UserItemsToExecute().Select(Execute));

    private async Task ExecuteGroupMessages() 
        => await Task.WhenAll(GroupItemsToExecute().Select(Execute));

    private async Task Execute(QueueItem item)
    {
        try
        {
            await item.TaskGenerator();
        }
        catch (ApiRequestException e)
        {
            // https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once
            if (e.ErrorCode != 429)
                log.LogError($"Telegram api error: {e.ToPrettyJson()}");
            else
            {
                log.LogWarning("Api limit is already reached!");

                if (item.GroupChatId.HasValue)
                    groupIdToQueue[item.GroupChatId.Value].Queue.Enqueue(item);
                else
                    userMessageQueue.Enqueue(item);
            }
        }
        catch (Exception e)
        {
            log.LogError($"Error: {e.ToPrettyJson()}");
        }
    }

    private IEnumerable<QueueItem> UserItemsToExecute()
        => GetItemsToExecute(userMessageQueue, MessageBrokerSettings.UserMessagesLimit);

    private IEnumerable<QueueItem> GroupItemsToExecute()
    {
        foreach (var (groupId, (queue, sendPerMinuteCount)) in groupIdToQueue)
        {
            if (sendPerMinuteCount >= MessageBrokerSettings.GroupMessagesPerMinuteLimit)
                continue;

            var counter = sendPerMinuteCount;
            var allowedCount = MessageBrokerSettings.GroupMessagesPerMinuteLimit - sendPerMinuteCount;

            foreach (var item in GetItemsToExecute(queue, allowedCount))
            {
                counter++;
                yield return item;
            }

            groupIdToQueue.TryUpdate(groupId, (queue, counter), (queue, sendPerMinuteCount));
        }
    }

    private static IEnumerable<QueueItem> GetItemsToExecute(MessageQueue queue, int allowedCount)
    {
        foreach (var priority in Enum.GetValues<MessagePriority>())
        {
            while (!queue.IsEmpty(priority))
            {
                if (allowedCount <= 0)
                    yield break;
                if (!queue.TryDequeue(priority, out var taskToExecute) || taskToExecute == null)
                    continue;

                allowedCount -= 1;
                yield return taskToExecute;
            }
        }
    }

    #endregion
}