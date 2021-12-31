using System.Collections.Generic;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Services
{
    public interface IScheduleMessageService
    {
        bool AddMessageToUpdate(long chatId, (string Data, int MessageId, DayAndMonth WhenDelete) message);
        bool RemoveMessageToUpdate(long chatId);
        IEnumerator<KeyValuePair<long, (string Data, int MessageId, DayAndMonth WhenDelete)>> GetAll();
        void Reset();
    }
}