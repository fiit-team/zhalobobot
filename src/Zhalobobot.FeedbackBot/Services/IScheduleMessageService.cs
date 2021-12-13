using System.Collections.Generic;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Services
{
    public interface IScheduleMessageService
    {
        bool AddMessageToUpdate((long ChatId, string Data, int MessageId, DayAndMonth WhenDelete) message);
        bool RemoveMessageToUpdate((long ChatId, string Data, int MessageId, DayAndMonth WhenDelete) message);
        IEnumerator<(long ChatId, string Data, int MessageId, DayAndMonth WhenDelete)> GetAll();
        void Reset();
    }
}