using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback;
using Zhalobobot.Common.Models.Student;

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

        public void SaveFeedback(long chatId, string message)
        {
            if (!Conversations.TryGetValue(chatId, out var value))
            {
                Logger.LogError($"Conversation not found. ChatId {chatId}.");
                throw new Exception($"Conversation not found. ChatId {chatId}.");
            } 
            
            Conversations[chatId] = value with {Message = message};

            Logger.LogInformation($"Feedback saved successfully. ChatId {chatId}");
        }

        public void StopConversation(long chatId)
        {
            Conversations.Remove(chatId);

            Logger.LogInformation($"Conversation stopped successfully. ChatId {chatId}");
        }

        public void StartUrgentFeedback(long chatId)
        {
            Conversations[chatId] = Feedback.Urgent;

            Logger.LogInformation($"Urgent feedback started successfully. ChatId {chatId}");
        }

        public void StartGeneralFeedback(long chatId)
        {
            Conversations[chatId] = Feedback.General;

            Logger.LogInformation($"General feedback started successfully. ChatId {chatId}");
        }

        public void StartSubjectFeedback(long chatId, string subjectName)
        {
            Conversations[chatId] = Feedback.Subj with { Subject = new Subject(subjectName) };

            Logger.LogInformation($"Subject feedback started successfully. ChatId {chatId}, Subject {subjectName}");
        }

        public async Task SendFeedbackAsync(long chatId, AbTestStudent student)
        {
            if (!Conversations.TryGetValue(chatId, out var feedback)
                && feedback?.Message is null)
            {
                Logger.LogError($"Chat not found or no feedback message. ChatId {chatId}");
                throw new Exception($"Chat not found or no feedback message. ChatId {chatId}");
            }
            
            if (feedback.Type == FeedbackType.UrgentFeedback)
            {
                await SendUrgentFeedback(feedback.Message);
            }

            await Client.Feedback.AddFeedback(feedback with { Student = student });
            Logger.LogInformation($"Saved feedback in repository.");

            Conversations.Remove(chatId);
        }

        public ConversationStatus GetConversationStatus(long chatId)
        {
            return Conversations.TryGetValue(chatId, out var value)
                ? string.IsNullOrEmpty(value.Message)
                    ? ConversationStatus.AwaitingFeedback
                    : ConversationStatus.AwaitingConfirmation
                : ConversationStatus.Default;
        }

        private async Task SendUrgentFeedback(string message)
        {
            var text = $"Алерт! Кто-то оставил срочную обратную связь:\n\n" +
                        $"\"{message}\"";

            await BotClient.SendTextMessageAsync(Settings.UrgentFeedbackChatId, text);

            Logger.LogInformation("Sent urgent feedback successfully.");
        }
    }
}
