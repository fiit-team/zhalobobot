using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Schedule;
using Zhalobobot.Common.Models.Commons;
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
        public async Task<ScheduleItem[]> GetAll()
            => (await repository.GetAll()).ToArray();

        [HttpPost("holidays")]
        public async Task<DayAndMonth[]> GetHolidays() 
            => await repository.GetHolidays();
    }
}