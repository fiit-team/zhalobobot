using System.Threading.Tasks;

namespace Zhalobobot.Api.Server.Repositories.Schedule
{
    public interface IScheduleRepository
    {
        Task<ScheduleItem[]?> ParseSchedule();
    }
}