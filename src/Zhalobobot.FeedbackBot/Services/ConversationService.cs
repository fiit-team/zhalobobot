using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback;
using Zhalobobot.Common.Models.Feedback.Requests;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Bot.Services
{
    public class ConversationService : IConversationService
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotApiClient Client { get; }
        private Settings Settings { get; }
        private EntitiesCache Cache { get; }
        private ILogger Logger { get; }

        private IDictionary<long, Conversation> Conversations { get; }
            = new ConcurrentDictionary<long, Conversation>();

        public ConversationService(
            ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            Settings settings,
            EntitiesCache cache,
            ILogger<ConversationService> logger)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Cache = cache;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SaveMessage(long chatId, string message)
        {
            if (!Conversations.TryGetValue(chatId, out var value))
            {
                Logger.LogError($"Conversation not found. ChatId {chatId}.");
                throw new Exception($"Conversation not found. ChatId {chatId}.");
            }

            value.Messages.Add(message);

            Logger.LogInformation($"Message saved successfully. ChatId {chatId}");
        }

        public void StopConversation(long chatId)
        {
            Conversations.Remove(chatId);

            Logger.LogInformation($"Conversation stopped successfully. ChatId {chatId}");
        }

        public void StartUrgentFeedback(long chatId)
        {
            var student = Cache.Students.Get(chatId);
            
            Conversations[chatId] = new Conversation
            {
                Feedback = new Feedback(FeedbackType.Urgent, student)
            }; 

            Logger.LogInformation($"Urgent feedback started successfully. ChatId {chatId}");
        }

        public void StartGeneralFeedback(long chatId)
        {
            var student = Cache.Students.Get(chatId);
            
            Conversations[chatId] = new Conversation
            {
                Feedback = new Feedback(FeedbackType.General, student)
            };

            Logger.LogInformation($"General feedback started successfully. ChatId {chatId}");
        }

        public void StartSubjectFeedback(long chatId, string subjectName)
        {
            var student = Cache.Students.Get(chatId);

            var subject = Cache.Subjects
                .Get(subjectName)
                .First(s => s.Course == student.Course && s.Semester == SemesterHelper.Current);
            
            var feedback = new Feedback(FeedbackType.Subject, student, null, subject, new SubjectSurvey());

            Conversations[chatId] = new Conversation
            {
                Feedback = feedback
            };

            Logger.LogInformation($"Subject feedback started successfully. ChatId {chatId}, Subject {subjectName}");
        }

        public async Task SendFeedbackAsync(long chatId)
        {
            if (!Conversations.TryGetValue(chatId, out var value))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            var student = Cache.Students.Get(chatId);

            var feedback = value.Feedback with { Student = student };

            if (feedback.Type == FeedbackType.Urgent)
            {
                var message = string.Join("\n", value.Messages);
                await SendUrgentFeedback(message, student).ConfigureAwait(false);
            }
            
            await SaveStructuredFeedback(chatId, feedback).ConfigureAwait(false);

            Conversations.Remove(chatId);
        }

        public ConversationStatus GetConversationStatus(long chatId)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
                return ConversationStatus.Default;

            var feedback = conversation.Feedback;

            if (feedback.Type == FeedbackType.Subject)
            {
                var rating = feedback.SubjectSurvey!.Rating;
                if (rating == 0)
                    return ConversationStatus.AwaitingRating;

                if (rating >= 3 && feedback.SubjectSurvey!.LikedPoints.Count == 0)
                    return ConversationStatus.AwaitingLikedPointsPollAnswer;

                if (rating <= 3 && feedback.SubjectSurvey!.UnlikedPoints.Count == 0)
                    return ConversationStatus.AwaitingUnlikedPointsPollAnswer;

                return ConversationStatus.AwaitingConfirmation;
            }

            return conversation.Messages.Count == 0
                ? ConversationStatus.AwaitingMessage
                : ConversationStatus.AwaitingConfirmation;
        }

        public void SaveRating(long chatId, int rating)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            conversation.Feedback.SubjectSurvey!.Rating = rating;
        }

        public void SavePollInfo(long chatId, PollInfo pollInfo)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            conversation.LastPollInfo = pollInfo;
        }

        public PollInfo GetLastPollInfo(long chatId)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            return conversation.LastPollInfo;
        }

        public void ProcessPollAnswer(long chatId, ICollection<string> result, bool isLikedPoints = false)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            if (isLikedPoints)
                conversation.Feedback.SubjectSurvey!.LikedPoints = result;
            else
                conversation.Feedback.SubjectSurvey!.UnlikedPoints = result;
        }

        private async Task SaveStructuredFeedback(long chatId, Feedback feedback)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            foreach (var entity in conversation.Messages.Select(message => feedback with { Message = message }))
            {
                await Client.Feedback.AddFeedback(new AddFeedbackRequest(entity));
            }

            Logger.LogInformation($"Saved feedback in repository. ChatId {chatId}.");
        }

        private async Task SendUrgentFeedback(string message, Student student)
        {
            var builder = new StringBuilder();

            builder.AppendLine("@disturm"); // костыли любимые
            builder.AppendLine("Алерт! Кто-то оставил срочную обратную связь");
            builder.AppendLine();
            builder.AppendLine($"{student.Name ?? Name.UnknownPerson}");
            builder.AppendLine($"{student.Group}");

            if (student.Username?.Length > 1)
            {
                builder.AppendLine(student.Username);
            }

            builder.AppendLine();
            builder.AppendLine(message);

            await BotClient.SendTextMessageAsync(
                Settings.UrgentFeedbackChatId, 
                builder.ToString());

            Logger.LogInformation("Sent urgent feedback successfully.");
        }
    }
}
