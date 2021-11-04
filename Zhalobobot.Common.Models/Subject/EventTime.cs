using System;

namespace Zhalobobot.Common.Models.Subject
{
    public record EventTime(
        DayOfWeek DayOfWeek,
        int Pair, // номер пары
        string? StartTime, // для физры с 10:00, например, в привычный формат пар не укладывается
        string? EndTime,
        bool ExistsThisWeek); //если пара мигающая или просто на этой неделе отменена
}