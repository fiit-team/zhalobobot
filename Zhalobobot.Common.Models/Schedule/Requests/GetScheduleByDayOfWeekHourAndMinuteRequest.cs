using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Schedule.Requests
{
    public class GetScheduleByDayOfWeekHourAndMinuteRequest
    {
        public GetScheduleByDayOfWeekHourAndMinuteRequest(DayOfWeek dayOfWeek, HourAndMinute hourAndMinute)
        {
            DayOfWeek = dayOfWeek;
            HourAndMinute = hourAndMinute;
        }
        
        public DayOfWeek DayOfWeek { get; }
        public HourAndMinute HourAndMinute { get; }
    }
}