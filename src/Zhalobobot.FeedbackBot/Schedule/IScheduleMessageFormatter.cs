using System;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Schedule
{
    public interface IScheduleMessageFormatter
    {
        string Format(long chatId, ScheduleDay scheduleDay, out DateOnly? whenDelete);
    }
}