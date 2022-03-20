using System;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public interface IScheduleRepository
    {
        Task<Zhalobobot.Common.Models.Schedule.Schedule> GetAll();
        Task<DateOnly[]> GetHolidays();
        Task<DayWithoutPairs[]> GetDaysWithoutPairs();
    }
}