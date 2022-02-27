using System;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;

namespace Zhalobobot.Common.Clients.Schedule
{
    public interface IScheduleClient
    {
        Task<ZhalobobotResult<ScheduleItem[]>> GetAll();
        
        Task<ZhalobobotResult<DateOnly[]>> GetHolidays();
    }
}