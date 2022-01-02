using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Subjects;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("subjects")]
    public class SubjectsController
    {
        private readonly ISubjectRepository repository;

        public SubjectsController(ISubjectRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("getAll")]
        public async Task<Subject[]> GetAll()
            => await repository.GetAll();
    }
}