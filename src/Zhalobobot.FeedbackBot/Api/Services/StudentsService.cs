using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Bot.Api.Repositories.FiitStudentsData;
using Zhalobobot.Bot.Api.Repositories.Students;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Bot.Api.Services;

public class StudentsService
{
    private readonly IStudentRepository repository;
    private readonly IFiitStudentsDataRepository fiitStudentsDataRepository;

    public StudentsService(IStudentRepository repository, IFiitStudentsDataRepository fiitStudentsDataRepository)
    {
        this.repository = repository;
        this.fiitStudentsDataRepository = fiitStudentsDataRepository;
    }

    public async Task<Student[]> GetAll()
        => await repository.GetAll();

    public async Task<StudentData[]> GetAllData()
        => (await fiitStudentsDataRepository.GetAll()).ToArray();

    public async Task Add([FromBody] AddStudentRequest request)
        => await repository.Add(request.Student);

    public async Task Update([FromBody] UpdateStudentRequest request)
        => await repository.Update(request.Student);
}