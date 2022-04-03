using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Services
{
    public interface IConversationService
    {
        public void StopConversation(long chatId);

        public void StartUrgentFeedback(long chatId);

        public void StartGeneralFeedback(long chatId);

        public void StartSubjectFeedback(long chatId, string subjectName);
        
        public void StartAddSpecialCoursesFeedback(long chatId);

        public void SaveMessage(long chatId, Message message);

        public void SaveRating(long chatId, int rating);

        public void SavePollInfo(long chatId, PollInfo pollInfo);
        public void SaveSelectedSpecialCourses(long chatId, HashSet<string> selectedSpecialCourses);

        public PollInfo GetLastPollInfo(long chatId);

        public HashSet<string> GetSelectedSpecialCourses(long chatId);

        public bool HaveSelectedSpecialCourses(long chatId);

        public void ProcessPollAnswer(long chatId, ICollection<string> result, bool isLikedPoints = false);

        public Task SendFeedbackAsync(long chatId, int messageId);

        public ConversationStatus GetConversationStatus(long chatId);
    }
}
