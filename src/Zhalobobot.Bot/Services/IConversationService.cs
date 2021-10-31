using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Services
{
    public interface IConversationService
    {
        public void StopConversation(long chatId);

        public void StartUrgentFeedback(long chatId);

        public void StartGeneralFeedback(long chatId);

        public void StartSubjectFeedback(long chatId, string subjectName);

        public void SaveMessage(long chatId, string message);

        public void SaveRating(long chatId, int rating);

        public void SavePollInfo(long chatId, PollInfo pollInfo);

        public PollInfo GetLastPollInfo(long chatId);

        public void ProcessPollAnswer(long chatId, ICollection<string> result, bool isLikedPoints = false);

        public Task SendFeedbackAsync(long chatId, AbTestStudent student);

        public ConversationStatus GetConversationStatus(long chatId);
    }
}
