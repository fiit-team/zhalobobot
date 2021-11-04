using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Schedule;

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

        [HttpPost("get")]
        public async Task<ScheduleItem[]?> Get()
            => await repository.ParseSchedule();
    }
}