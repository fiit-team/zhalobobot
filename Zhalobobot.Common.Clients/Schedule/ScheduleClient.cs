using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Common.Clients.Schedule
{
    public class ScheduleClient : ClientBase, IScheduleClient
    {
        public ScheduleClient(HttpClient client, string serverUri) 
            : base("schedule", client, serverUri)
        {
        }

        public Task<ZhalobobotResult<Models.Schedule.Schedule>> GetAll()
            => Method<Models.Schedule.Schedule>("getAll").CallAsync();

        public Task<ZhalobobotResult<DayWithoutPairs[]>> GetDaysWithoutPairs()
            => Method<DayWithoutPairs[]>("daysWithoutPairs").CallAsync();
    }
}