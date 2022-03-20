using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Schedule;
using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("schedule")]
    public class ScheduleController
    {
        private readonly IScheduleRepository repository;

        public ScheduleController(IScheduleRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("getAll")]
        public async Task<Schedule> GetAll()
            => await repository.GetAll();

        [HttpPost("daysWithoutPairs")]
        public async Task<DayWithoutPairs[]> GetDaysWithoutPairs()
            => await repository.GetDaysWithoutPairs();
    }
}