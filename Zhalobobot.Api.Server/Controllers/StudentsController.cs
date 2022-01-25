using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Api.Server.Repositories.FiitStudentsData;
using Zhalobobot.Api.Server.Repositories.Students;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("students")]
    public class StudentsController
    {
        private readonly IStudentRepository repository;
        private readonly IFiitStudentsDataRepository fiitStudentsDataRepository;

        public StudentsController(IStudentRepository repository, IFiitStudentsDataRepository fiitStudentsDataRepository)
        {
            this.repository = repository;
            this.fiitStudentsDataRepository = fiitStudentsDataRepository;
        }

        [HttpPost("getAll")]
        public async Task<Student[]> GetAll()
            => await repository.GetAll();

        [HttpPost("getAllData")]
        public async Task<StudentData[]> GetAllData()
            => (await fiitStudentsDataRepository.GetAll()).ToArray();

        [HttpPost("add")]
        public async Task Add([FromBody] AddStudentRequest request)
            => await repository.Add(request.Student);
    }
}