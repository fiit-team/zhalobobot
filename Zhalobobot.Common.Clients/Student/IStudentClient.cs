using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Common.Clients.Student
{
    public interface IStudentClient
    {
        Task<ZhalobobotResult<Models.Student.Student[]>> GetAll();

        Task<ZhalobobotResult<StudentData[]>> GetAllData();
        
        Task<ZhalobobotResult> Add(AddStudentRequest request);
    }
}