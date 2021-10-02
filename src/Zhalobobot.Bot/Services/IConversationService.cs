using System.Threading.Tasks;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Services
{
    public interface IConversationService
    {
        public void StopConversation(long chatId);

        public void StartUrgentFeedback(long chatId);

        public void StartGeneralFeedback(long chatId);

        public void StartSubjectFeedback(long chatId, string subjectName);

        public void SaveFeedback(long chatId, string message);

        public Task SendFeedbackAsync(long chatId);

        public ConversationStatus GetConversationStatus(long chatId);
    }
}
