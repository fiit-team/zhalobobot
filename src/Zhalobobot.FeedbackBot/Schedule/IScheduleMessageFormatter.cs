using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Schedule
{
    public interface IScheduleMessageFormatter
    {
        string Format(long chatId, ScheduleDay scheduleDay, out DayAndMonth? whenDelete);
    }
}