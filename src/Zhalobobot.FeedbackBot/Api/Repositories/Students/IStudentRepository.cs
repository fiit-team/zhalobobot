using System.Threading.Tasks;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Api.Repositories.Students;

public interface IStudentRepository
{
    Task Add(Student student);

    Task Update(Student student);
        
    Task<Student[]> GetAll();
}