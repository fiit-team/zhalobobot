using System.Threading.Tasks;

namespace Zhalobobot.Common.Clients.Core
{
    public static class ZhalobobotResultExtensions
    {
        public static async Task<T?> GetResult<T>(this Task<ZhalobobotResult<T>> task)
            => (await task).Result;
    }
}