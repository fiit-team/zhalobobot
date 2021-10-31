using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Subjects;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("subjects")]
    public class SubjectsController
    {
        private readonly ISubjectsRepository repository;

        public SubjectsController(ISubjectsRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public async Task<Subject[]> Get()
            => await repository.Get();
    }
}