using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Schedule.Requests;

namespace Zhalobobot.Common.Clients.Schedule
{
    public class ScheduleClient : ClientBase, IScheduleClient
    {
        public ScheduleClient(HttpClient client, string serverUri) 
            : base("schedule", client, serverUri)
        {
        }

        public Task<ZhalobobotResult<ScheduleItem[]>> GetByCourse(GetScheduleByCourseRequest request)
            => Method<ScheduleItem[]>("getByCourse").CallAsync(request);

        public Task<ZhalobobotResult<ScheduleItem[]>> GetByDayOfWeek(GetScheduleByDayOfWeekRequest request)
            => Method<ScheduleItem[]>("getByDayOfWeek").CallAsync(request);

        public Task<ZhalobobotResult<ScheduleItem[]>> GetByDayOfWeekAndStartsAtHourAndMinute(GetScheduleByDayOfWeekHourAndMinuteRequest request)
            => Method<ScheduleItem[]>("getByDayOfWeekAndStartsAtHourAndMinute").CallAsync(request);

        public Task<ZhalobobotResult<ScheduleItem[]>> GetByDayOfWeekAndEndsAtHourAndMinute(GetScheduleByDayOfWeekHourAndMinuteRequest request)
            => Method<ScheduleItem[]>("getByDayOfWeekAndEndsAtHourAndMinute").CallAsync(request);
    }
}