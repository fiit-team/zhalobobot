using System.Threading.Tasks;
using Zhalobobot.Common.Models.Reply;

namespace Zhalobobot.Bot.Api.Repositories.Replies;

public interface IReplyRepository
{
    /// <summary>
    /// �������� ����� �� �������� �����.
    /// </summary>
    /// <param name="reply"></param>
    /// <returns></returns>
    public Task Add(Reply reply);

    /// <summary>
    /// �������� ��� ������ �� �������� �����.
    /// </summary>
    /// <returns></returns>
    Task<Reply[]> GetAll();
}