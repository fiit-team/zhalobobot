using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.AbTest;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("students")]
    public class StudentsController
    {
        private readonly IAbTestRepository abTestRepository;

        public StudentsController(IAbTestRepository abTestRepository)
        {
            this.abTestRepository = abTestRepository;
        }

        [HttpGet("abTest/{telegramId}")]
        public async Task<AbTestStudent> GetAbTestStudent([FromRoute] string telegramId)
        {
            return await abTestRepository.Get(telegramId);
        }
    }
}