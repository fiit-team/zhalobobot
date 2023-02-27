using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Api.Repositories.FiitStudentsData;

public interface IFiitStudentsDataRepository
{
    Task<IEnumerable<StudentData>> GetAll();
}