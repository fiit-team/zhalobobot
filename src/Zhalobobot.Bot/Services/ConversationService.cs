using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Bot.Services
{
    public class ConversationService : IConversationService
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotApiClient Client { get; }
        private Settings Settings { get; }
        private ILogger Logger { get; }

        private IDictionary<long, Feedback> Conversations { get; }
            = new ConcurrentDictionary<long, Feedback>();

        private IDictionary<long, List<string>> Messages { get; }
            = new ConcurrentDictionary<long, List<string>>();

        public ConversationService(
            ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            Settings settings,
            ILogger<ConversationService> logger)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SaveMessage(long chatId, string message)
        {
            if (!Messages.TryGetValue(chatId, out var value))
            {
                Logger.LogError($"Conversation not found. ChatId {chatId}.");
                throw new Exception($"Conversation not found. ChatId {chatId}.");
            }

            value.Add(message);

            Logger.LogInformation($"Message saved successfully. ChatId {chatId}");
        }

        public void StopConversation(long chatId)
        {
            Conversations.Remove(chatId);
            Messages.Remove(chatId);

            Logger.LogInformation($"Conversation stopped successfully. ChatId {chatId}");
        }

        public void StartUrgentFeedback(long chatId)
        {
            Conversations[chatId] = Feedback.Urgent;
            Messages[chatId] = new List<string>();

            Logger.LogInformation($"Urgent feedback started successfully. ChatId {chatId}");
        }

        public void StartGeneralFeedback(long chatId)
        {
            Conversations[chatId] = Feedback.General;
            Messages[chatId] = new List<string>();

            Logger.LogInformation($"General feedback started successfully. ChatId {chatId}");
        }

        public void StartSubjectFeedback(long chatId, string subjectName)
        {
            Conversations[chatId] = Feedback.Subj with { Subject = new Subject(subjectName) };
            Messages[chatId] = new List<string>();

            Logger.LogInformation($"Subject feedback started successfully. ChatId {chatId}, Subject {subjectName}");
        }

        public async Task SendFeedbackAsync(long chatId, AbTestStudent student)
        {
            if (!Conversations.TryGetValue(chatId, out var feedback))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            feedback = feedback with { Student = student };

            if (feedback.Type == FeedbackType.UrgentFeedback)
            {
                var message = string.Join("\n\n", Messages[chatId]);
                await SendUrgentFeedback(message, student).ConfigureAwait(false);
                await SaveUnStructuredFeedback(chatId, feedback).ConfigureAwait(false);
            }
            else
            {
                if (student.InGroupA)
                    await SaveStructuredFeedback(chatId, feedback).ConfigureAwait(false);
                else
                    await SaveUnStructuredFeedback(chatId, feedback).ConfigureAwait(false);
            }

            Conversations.Remove(chatId);
            Messages.Remove(chatId);
        }

        public ConversationStatus GetConversationStatus(long chatId)
        {
            return Messages.TryGetValue(chatId, out var value)
                ? value.Count == 0
                    ? ConversationStatus.AwaitingFeedback
                    : ConversationStatus.AwaitingConfirmation
                : ConversationStatus.Default;
        }

        private async Task SaveStructuredFeedback(long chatId, Feedback feedback)
        {
            foreach (var entity in Messages[chatId]
                .Select(message => feedback with { Message = message }))
            {
                await Client.Feedback.AddFeedback(entity);
            }

            Logger.LogInformation($"Saved feedback in repository. ChatId {chatId}.");
        }

        private async Task SaveUnStructuredFeedback(long chatId, Feedback feedback)
        {
            feedback = feedback with { Message = string.Join("\n", Messages[chatId]) };

            await Client.Feedback.AddFeedback(feedback);

            Logger.LogInformation($"Saved feedback in repository. ChatId {chatId}.");
        }

        private async Task SendUrgentFeedback(string message, Student student)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Алерт! Кто-то оставил срочную обратную связь");
            builder.AppendLine();
            builder.AppendLine($"{student.Name ?? Name.UnknownPerson}");
            builder.AppendLine();

            var group = student.GetGroup();
            if (group != null)
            {
                builder.AppendLine($"{group}");
                builder.AppendLine();
            }

            builder.AppendLine(message);

            await BotClient.SendTextMessageAsync(
                Settings.UrgentFeedbackChatId, 
                builder.ToString());

            Logger.LogInformation("Sent urgent feedback successfully.");
        }
    }
}
