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

        [HttpPost("find/{telegramId}")]
        public async Task<Student?> FindStudent(string telegramId)
        {
            var result = await repository.FindById(long.Parse(telegramId));
            
            // TODO: Возвращать ActionResult
            // TODO: if null return NotFound

            return result;
        }

        [HttpPost("getAll")]
        public async Task<Student[]> GetAll()
            => await repository.GetAll();

        [HttpPost("getByCourseAndGroupAndSubgroup")]
        public async Task<Student[]> GetByCourseAndGroupAndSubgroup([FromBody] GetStudentsByCourseAndGroupAndSubgroupRequest request)
            => await repository.GetByCourseAndGroupAndSubgroup(request.Course, request.Group, request.Subgroup);

        [HttpPost("add")]
        public async Task Add([FromBody] AddStudentRequest request)
            => await repository.Add(request.Student);
    }
}