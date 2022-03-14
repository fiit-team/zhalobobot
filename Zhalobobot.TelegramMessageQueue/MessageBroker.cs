using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Vostok.Commons.Time;
using Zhalobobot.TelegramMessageQueue.Core;
using Zhalobobot.TelegramMessageQueue.Models;
using Zhalobobot.TelegramMessageQueue.Settings;

namespace Zhalobobot.TelegramMessageQueue;

public class MessageBroker : IDisposable
{
    private readonly MessageQueue userMessageQueue;
    private readonly ConcurrentDictionary<string, (MessageQueue Queue, int SendCountPerMinute)> groupIdToQueue;
    private readonly AsyncPeriodicalAction clearSendCountRoutine;
    private readonly AsyncPeriodicalAction executeUserMessagesRoutine;
    private readonly AsyncPeriodicalAction executeGroupMessagesRoutine;
    private readonly ILogger<MessageBroker> log;

    private MessageBroker(ILogger<MessageBroker> log)
    {
        this.log = log;
        userMessageQueue = new MessageQueue();
        groupIdToQueue = new ConcurrentDictionary<string, (MessageQueue Queue, int SendCountPerMinute)>();
        clearSendCountRoutine = new AsyncPeriodicalAction(ClearSendCountPerMinute, LogError, () => TimeSpan.FromMinutes(1));
        executeUserMessagesRoutine = new AsyncPeriodicalAction(ExecuteUserMessages, LogError, () => TimeSpan.FromSeconds(1));
        executeGroupMessagesRoutine = new AsyncPeriodicalAction(ExecuteGroupMessages, LogError, () => TimeSpan.FromSeconds(1));
        
        clearSendCountRoutine.Start();
        executeUserMessagesRoutine.Start();
        executeGroupMessagesRoutine.Start();

        async Task ClearSendCountPerMinute()
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

        void LogError(Exception e) => this.log.LogError(e.Message);

        async Task ExecuteUserMessages() 
            => await Task.WhenAll(UserTasksToExecute().Select(Catch429Error));
        
        async Task ExecuteGroupMessages() 
            => await Task.WhenAll(GroupTasksToExecute().Select(Catch429Error));

        async Task Catch429Error(Task taskToExecute)
        {
            try
            {
                await taskToExecute;
            }
            catch (ApiRequestException e)
            {
                // https://core.telegram.org/bots/faq#how-can-i-message-all-of-my-bot-39s-subscribers-at-once
                if (e.ErrorCode != 429)
                    throw;
            
                log.LogWarning("Api limit is already reached!");
            }
        }
    }

    public void EnqueueToUsers(MessagePriority priority, Task<Message> taskToExecute)
        => userMessageQueue.Enqueue(priority, taskToExecute);

    public void EnqueueToGroups(string groupId, MessagePriority priority, Task<Message> taskToExecute)
    {
        if (!groupIdToQueue.ContainsKey(groupId))
            groupIdToQueue[groupId] = (new MessageQueue(), 0);
        
        groupIdToQueue[groupId].Queue.Enqueue(priority, taskToExecute);
    }

    internal IEnumerable<Task<Message>> UserTasksToExecute()
    {
        var allowedCount = MessageBrokerSettings.UserMessagesLimit;
        
        foreach (var priority in Enum.GetValues<MessagePriority>())
        {
            while (!userMessageQueue.IsEmpty(priority))
            {
                if (allowedCount <= 0)
                    yield break;
                if (!userMessageQueue.TryDequeue(priority, out var taskToExecute) || taskToExecute == null) 
                    continue;

                allowedCount -= 1;
                yield return taskToExecute;
            }
        }
    }

    private IEnumerable<Task<Message>> GroupTasksToExecute()
    {
        foreach (var (groupId, (queue, sendPerMinuteCount)) in groupIdToQueue)
        {
            if (sendPerMinuteCount >= MessageBrokerSettings.GroupMessagesPerMinuteLimit)
                continue;

            var counter = sendPerMinuteCount;

            foreach (var priority in Enum.GetValues<MessagePriority>())
            {
                while (!queue.IsEmpty(priority))
                {
                    if (counter >= MessageBrokerSettings.GroupMessagesPerMinuteLimit)
                        break;
                    if (!queue.TryDequeue(priority, out var taskToExecute) || taskToExecute == null) 
                        continue;

                    counter += 1;
                    yield return taskToExecute;
                }
                
                if (counter >= MessageBrokerSettings.GroupMessagesPerMinuteLimit)
                    break;
            }

            groupIdToQueue.TryUpdate(groupId, (queue, counter), (queue, sendPerMinuteCount));
        }
    }

    public void Dispose()
    {
        clearSendCountRoutine.Stop();
        executeUserMessagesRoutine.Stop();
        executeGroupMessagesRoutine.Stop();
    }
}