using System.Collections.Concurrent;
using System.Collections.Generic;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Services
{
    public class ScheduleMessageService : IScheduleMessageService
    {
        private ConcurrentDictionary<long, (string Data, int MessageId, DayAndMonth WhenDelete)> messageToUpdate = new();

        public bool AddMessageToUpdate(long chatId, (string Data, int MessageId, DayAndMonth WhenDelete) message)
        {
            if (messageToUpdate.TryAdd(chatId, message))
                return true;

            return messageToUpdate.TryRemove(chatId, out _) && messageToUpdate.TryAdd(chatId, message);
        }
        
        public bool RemoveMessageToUpdate(long chatId) 
            => messageToUpdate.TryRemove(chatId, out _);

        public IEnumerator<KeyValuePair<long, (string Data, int MessageId, DayAndMonth WhenDelete)>> GetAll() => messageToUpdate.GetEnumerator();

        public void Reset() => messageToUpdate = new ConcurrentDictionary<long, (string Data, int MessageId, DayAndMonth WhenDelete)>();
    }
}