using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Common.Clients.Schedule
{
    public interface IScheduleClient
    {
        Task<ZhalobobotResult<ScheduleItem[]>> GetAll();
        
        Task<ZhalobobotResult<DayAndMonth[]>> GetHolidays();
    }
}