using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Common.Clients.Core.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T?> As<T>(this HttpResponseMessage message)
        {
            return !message.IsSuccessStatusCode 
                ? default 
                : (await message.Content.ReadAsStreamAsync()).FromJsonStream<T>();
        }
    }
}