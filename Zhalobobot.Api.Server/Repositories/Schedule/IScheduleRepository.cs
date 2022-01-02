using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<ScheduleItem>> GetAll();
        Task<DayAndMonth[]> GetHolidays();
    }
}