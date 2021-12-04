using System.Threading.Tasks;

namespace Zhalobobot.Bot.Cache
{
    public interface IEntityCacheContainer
    {
        Task Update(bool ensureSuccess);
    }
}