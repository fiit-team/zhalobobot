using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Subjects;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.Subject.Requests;

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

        [HttpPost("get")]
        public async Task<Subject[]> Get([FromBody] GetSubjectsRequest request)
        {
            request ??= new GetSubjectsRequest();

            return await repository.Get(request.Course, request.Category);
        }
    }
}