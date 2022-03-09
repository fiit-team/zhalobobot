using System.Threading.Tasks;
using Zhalobobot.Common.Models.Reply;

namespace Zhalobobot.Api.Server.Repositories.Feedback
{
    public interface IReplyRepository
    {
        /// <summary>
        /// Добавить ответ на обратную связь.
        /// </summary>
        /// <param name="reply"></param>
        /// <returns></returns>
        public Task Add(Reply reply);

        /// <summary>
        /// Получить все ответы на обратную связь.
        /// </summary>
        /// <returns></returns>
        Task<Reply[]> GetAll();
    }
}