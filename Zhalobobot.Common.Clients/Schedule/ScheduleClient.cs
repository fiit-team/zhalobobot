using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Common.Clients.Schedule
{
    public class ScheduleClient : ClientBase, IScheduleClient
    {
        public ScheduleClient(HttpClient client, string serverUri) 
            : base("schedule", client, serverUri)
        {
        }

        public Task<ZhalobobotResult<ScheduleItem[]>> GetAll()
            => Method<ScheduleItem[]>("getAll").CallAsync();

        public Task<ZhalobobotResult<DateOnly[]>> GetHolidays()
            => Method<DateOnly[]>("holidays").CallAsync();
    }
}