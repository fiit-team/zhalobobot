using System.Collections.Generic;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Services
{
    public class ScheduleMessageService : IScheduleMessageService
    {
        private ConcurrentSet<(long ChatId, string Data, int MessageId, DayAndMonth WhenDelete)> messageToUpdate = new();

        public bool AddMessageToUpdate((long ChatId, string Data, int MessageId, DayAndMonth WhenDelete) message) 
            => messageToUpdate.Add(message);

        public bool RemoveMessageToUpdate((long ChatId, string Data, int MessageId, DayAndMonth WhenDelete) message) 
            => messageToUpdate.Remove(message);

        public IEnumerator<(long ChatId, string Data, int MessageId, DayAndMonth WhenDelete)> GetAll() => messageToUpdate.GetEnumerator();

        public void Reset() => messageToUpdate = new ConcurrentSet<(long ChatId, string Data, int MessageId, DayAndMonth WhenDelete)>();
    }
}