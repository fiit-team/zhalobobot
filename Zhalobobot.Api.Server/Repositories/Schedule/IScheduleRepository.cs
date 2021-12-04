using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<ScheduleItem>> GetAll();
        Task<ScheduleItem[]> GetByCourse(Course course);
        Task<ScheduleItem[]> GetByDayOfWeek(DayOfWeek dayOfWeek);
        Task<ScheduleItem[]> GetByDayOfWeekAndStartsAtHourAndMinute(DayOfWeek dayOfWeek, HourAndMinute hourAndMinute);
        Task<ScheduleItem[]> GetByDayOfWeekAndEndsAtHourAndMinute(DayOfWeek dayOfWeek, HourAndMinute hourAndMinute);
    }
}