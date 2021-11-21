using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject
{
    public record EventTime(
        DayOfWeek DayOfWeek,
        Pair? Pair, // номер пары
        HourAndMinute? StartTime, // для физры с 10:00, например, в привычный формат пар не укладывается
        HourAndMinute? EndTime,
        DayAndMonth? StartDay,
        DayAndMonth? EndDay,
        WeekParity WeekParity); //,    //если пара мигающая
    // bool NotExistsNextTime,    // если отменили следующую пару
    // bool IsOneTimeEvent,
    // bool CanceledForever);
}