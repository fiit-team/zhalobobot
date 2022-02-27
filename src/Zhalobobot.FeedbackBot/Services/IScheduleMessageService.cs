using System;
using System.Collections.Generic;

namespace Zhalobobot.Bot.Services
{
    public interface IScheduleMessageService
    {
        bool AddMessageToUpdate(long chatId, (string Data, int MessageId, DateOnly WhenDelete) message);
        bool RemoveMessageToUpdate(long chatId);
        IEnumerator<KeyValuePair<long, (string Data, int MessageId, DateOnly WhenDelete)>> GetAll();
        void Reset();
    }
}