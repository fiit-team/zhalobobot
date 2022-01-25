using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Api.Server.Repositories.FiitStudentsData
{
    public interface IFiitStudentsDataRepository
    {
        Task<IEnumerable<StudentData>> GetAll();
    }
}