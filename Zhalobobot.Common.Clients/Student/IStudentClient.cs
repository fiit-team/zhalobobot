using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Common.Clients.Student
{
    public interface IStudentClient
    {
        Task<ZhalobobotResult<AbTestStudent>> GetAbTestStudent(GetAbTestStudentRequest request);

        Task<ZhalobobotResult<Models.Student.Student?>> GetStudent(long telegramId);

        Task<ZhalobobotResult<Models.Student.Student[]>> GetAll();
        
        Task<ZhalobobotResult<Models.Student.Student[]>> Get(GetStudentsByCourseAndGroupAndSubgroupRequest request);
        
        Task<ZhalobobotResult> Add(AddStudentRequest request);
    }
}