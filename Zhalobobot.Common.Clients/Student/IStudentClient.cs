using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Common.Clients.Student
{
    public interface IStudentClient
    {
        Task<ZhalobobotResult<AbTestStudent>> GetAbTestStudent(string telegramId);
    }
}