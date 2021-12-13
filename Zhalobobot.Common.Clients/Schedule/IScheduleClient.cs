using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Schedule.Requests;

namespace Zhalobobot.Common.Clients.Schedule
{
    public interface IScheduleClient
    {
        Task<ZhalobobotResult<ScheduleItem[]>> GetAll();
        
        Task<ZhalobobotResult<ScheduleItem[]>> GetByCourse(GetScheduleByCourseRequest request);
        
        Task<ZhalobobotResult<ScheduleItem[]>> GetByDayOfWeek(GetScheduleByDayOfWeekRequest request);

        Task<ZhalobobotResult<ScheduleItem[]>> GetByDayOfWeekAndStartsAtHourAndMinute(GetScheduleByDayOfWeekHourAndMinuteRequest request);

        Task<ZhalobobotResult<ScheduleItem[]>> GetByDayOfWeekAndEndsAtHourAndMinute(GetScheduleByDayOfWeekHourAndMinuteRequest request);

        Task<ZhalobobotResult<DayAndMonth[]>> GetHolidays();
    }
}