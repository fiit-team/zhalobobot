using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Zhalobobot.Bot.Services.Handlers
{
    public interface IUpdateHandler
    {
        public bool Accept(Update update);

        public Task HandleUpdate(Update update);
    }
}
