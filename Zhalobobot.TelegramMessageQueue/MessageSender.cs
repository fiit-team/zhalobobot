using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Vostok.Commons.Time;
using Zhalobobot.Bot.Models.Exceptions;
using Zhalobobot.Common.Models.Serialization;
using Zhalobobot.TelegramMessageQueue.Core;
using Zhalobobot.TelegramMessageQueue.Exceptions;
using Zhalobobot.TelegramMessageQueue.Settings;

namespace Zhalobobot.TelegramMessageQueue;

public class MessageSender : IDisposable
{
    private readonly MessageQueue userMessageQueue;
    private readonly ConcurrentDictionary<long, (MessageQueue Queue, int SendCountPerMinute)> groupIdToQueue;
    private readonly PeriodicalAction clearSendCountRoutine;
    private readonly AsyncPeriodicalAction executeUserMessagesRoutine;
    private readonly AsyncPeriodicalAction executeGroupMessagesRoutine;
    private readonly ILogger<MessageSender> log;
    private readonly MessageSenderSettings settings;

    public MessageSender(MessageSenderSettings settings, ILogger<MessageSender> log)
    {
        this.settings = settings;
        this.log = log;
        userMessageQueue = new MessageQueue();
        groupIdToQueue = new ConcurrentDictionary<long, (MessageQueue Queue, int SendCountPerMinute)>();
        clearSendCountRoutine = new PeriodicalAction(ClearSendCountPerMinute, LogError, () => TimeSpan.FromMinutes(1));

        var executionTimeoutInSeconds = (double)1 / MessageSenderSettings.SenderExecutionCallsPerSecondCount;
        executeUserMessagesRoutine = new AsyncPeriodicalAction(ExecuteUserMessages, LogError, () => TimeSpan.FromSeconds(executionTimeoutInSeconds));
        executeGroupMessagesRoutine = new AsyncPeriodicalAction(ExecuteGroupMessages, LogError, () => TimeSpan.FromSeconds(executionTimeoutInSeconds));
        
        clearSendCountRoutine.Start();
        executeUserMessagesRoutine.Start();
        executeGroupMessagesRoutine.Start();
    }

    public void SendToUser(Func<Task> taskGenerator, MessagePriority priority = MessagePriority.Normal)
        => userMessageQueue.Enqueue(new QueueItem(priority, taskGenerator, null));

    public void SendToGroupChat(long groupId, Func<Task> taskGenerator, MessagePriority priority = MessagePriority.Normal)
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
        foreach (var (groupId, items) in groupIdToQueue)
        {
            var oldResult = items;
            var foundValue = true;
            while (true)
            {
                if (!foundValue || oldResult.SendCountPerMinute == 0)
                    break;
                if (groupIdToQueue.TryUpdate(groupId, (oldResult.Queue, 0), oldResult))
                    break;
                foundValue = groupIdToQueue.TryGetValue(groupId, out oldResult);
            }    
        }
    }
    
    private void LogError(Exception e) => log.LogError(e.Message);
    
    private async Task ExecuteUserMessages() 
        => await Task.WhenAll(UserItemsToExecute(settings.AllUserMessagesPerSecondLimit).Select(Execute));

    private async Task ExecuteGroupMessages() 
        => await Task.WhenAll(GroupItemsToExecute(settings.GroupMessagesPerMinuteLimit).Select(Execute));

    private async Task Execute(QueueItem item)
    {
        try
        {
            log.LogInformation("Try execute..");
            await item.TaskGenerator();
            log.LogInformation("Successful execution.");
        }
        catch (ChatNotFoundException)
        {
            log.LogWarning("Chat not found.");
        }
        catch (MessageIsNotModifiedException)
        {
            log.LogWarning("Message not modified.");
        }
        catch (ApiRequestException e)
        {
            // https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once
            if (e.ErrorCode == 429)
            {
                log.LogWarning("Api limit is already reached!");
                
                // note (d.stukov): fix the ability to call the same task until the end of the message limit
                await Task.Delay(500);

                if (item.GroupChatId.HasValue)
                {
                    log.LogInformation($"Enqueue in queue for {item.GroupChatId.Value} group..");
                    groupIdToQueue[item.GroupChatId.Value].Queue.Enqueue(item);
                }
                else
                {
                    log.LogInformation("Enqueue in users queue..");
                    userMessageQueue.Enqueue(item);
                }
            }
            else
            {
                log.LogError($"Telegram api error: {e.ToPrettyJson()}");
            }
        }
        catch (Exception e)
        {
            log.LogError($"Error: {e.ToPrettyJson()}");
        }
    }

    private IEnumerable<QueueItem> UserItemsToExecute(int limit)
        => userMessageQueue.GetItemsToExecute(limit);

    private IEnumerable<QueueItem> GroupItemsToExecute(int limitPerGroup)
    {
        foreach (var (groupId, (queue, sendPerMinuteCount)) in groupIdToQueue)
        {
            if (sendPerMinuteCount >= limitPerGroup)
                continue;

            var counter = sendPerMinuteCount;
            var allowedCount = limitPerGroup - sendPerMinuteCount;

            foreach (var item in queue.GetItemsToExecute(allowedCount))
            {
                counter++;
                yield return item;
            }

            groupIdToQueue.TryUpdate(groupId, (queue, counter), (queue, sendPerMinuteCount));
        }
    }

    #endregion
}