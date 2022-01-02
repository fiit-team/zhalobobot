using System.Threading.Tasks;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Api.Server.Repositories.Students
{
    public interface IStudentRepository
    {
        Task Add(Student student);
        
        Task<Student[]> GetAll();
    }
}