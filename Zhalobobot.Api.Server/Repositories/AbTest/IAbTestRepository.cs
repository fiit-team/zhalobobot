using System.Threading.Tasks;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Api.Server.Repositories.AbTest
{
    public interface IAbTestRepository
    {
        Task<AbTestStudent> Get(string telegramId);
    }
}