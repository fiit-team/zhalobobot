using System.Threading.Tasks;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Api.Server.Repositories.Students
{
    public interface IStudentRepository
    {
        Task Add(Student student);

        Task<Student?> GetById(string telegramId);

        Task<Student[]> GetAll();

        Task<Student[]> GetByCourseAndGroupAndSubgroup(Course course, Group group, Subgroup subgroup);
    }
}