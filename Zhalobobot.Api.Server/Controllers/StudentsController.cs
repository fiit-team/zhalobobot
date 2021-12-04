using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.AbTest;
using Zhalobobot.Api.Server.Repositories.Students;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("students")]
    public class StudentsController
    {
        private readonly IAbTestRepository abTestRepository;
        private readonly IStudentRepository studentRepository;

        public StudentsController(IAbTestRepository abTestRepository, IStudentRepository studentRepository)
        {
            this.abTestRepository = abTestRepository;
            this.studentRepository = studentRepository;
        }

        [HttpPost("ab-test")]
        public async Task<AbTestStudent> GetAbTestStudent([FromBody] GetAbTestStudentRequest request)
            => await abTestRepository.Get(request.Username);


        [HttpPost("find/{telegramId}")]
        public async Task<Student?> FindStudent(string telegramId)
        {
            var result = await studentRepository.FindById(long.Parse(telegramId));
            
            // TODO: Возвращать ActionResult
            // TODO: if null return NotFound

            return result;
        }

        [HttpPost("getAll")]
        public async Task<Student[]> GetAll()
            => await studentRepository.GetAll();

        [HttpPost("getByCourseAndGroupAndSubgroup")]
        public async Task<Student[]> GetByCourseAndGroupAndSubgroup([FromBody] GetStudentsByCourseAndGroupAndSubgroupRequest request)
            => await studentRepository.GetByCourseAndGroupAndSubgroup(request.Course, request.Group, request.Subgroup);

        [HttpPost("add")]
        public async Task Add([FromBody] AddStudentRequest request)
            => await studentRepository.Add(request.Student);
    }
}