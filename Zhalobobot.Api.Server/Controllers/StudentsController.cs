using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.Students;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("students")]
    public class StudentsController
    {
        private readonly IStudentRepository repository;

        public StudentsController(IStudentRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("getAll")]
        public async Task<Student[]> GetAll()
            => await repository.GetAll();

        [HttpPost("add")]
        public async Task Add([FromBody] AddStudentRequest request)
            => await repository.Add(request.Student);
    }
}