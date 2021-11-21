using System;

namespace Zhalobobot.Common.Models.Schedule.Requests
{
    public class GetScheduleByDayOfWeekRequest
    {
        public GetScheduleByDayOfWeekRequest(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
        }
        
        public DayOfWeek DayOfWeek { get; }
    }
}