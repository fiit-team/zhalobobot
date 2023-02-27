using System.Threading.Tasks;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Api.Repositories.Subjects;

public interface ISubjectRepository
{
    Task<Subject[]> GetAll();
}