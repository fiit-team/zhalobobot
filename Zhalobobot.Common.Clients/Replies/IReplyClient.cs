using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Reply.Requests;

namespace Zhalobobot.Common.Clients.Reply
{
    public interface IReplyClient
    {
        /// <summary>
        /// Добавить ответ на обратную связь.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<ZhalobobotResult> Add(AddReplyRequest request);

        /// <summary>
        /// Получить все ответы на обратную связь.
        /// </summary>
        /// <returns></returns>
        Task<ZhalobobotResult<Models.Reply.Reply[]>> GetAll();
    }
}