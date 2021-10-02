using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Zhalobobot.Bot.Models;
using Zhalobobot.Bot.Repositories;

namespace Zhalobobot.Bot.Services
{
    public class ConversationService : IConversationService
    {
        private ITelegramBotClient BotClient { get; }
        private IFeedbackRepository FeedbackRepository { get; }
        private Settings Settings { get; }
        private ILogger Logger { get; }

        private IDictionary<long, FeedbackInfo> Conversations { get; }
            = new ConcurrentDictionary<long, FeedbackInfo>();

        public ConversationService(
            ITelegramBotClient botClient,
            IFeedbackRepository feedbackRepository,
            Settings settings,
            ILogger<ConversationService> logger)
        {
            this.BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            this.FeedbackRepository = feedbackRepository
                ?? throw new ArgumentNullException(nameof(feedbackRepository));
            this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SaveFeedback(long chatId, string message)
        {
            if (!this.Conversations.TryGetValue(chatId, out var value))
            {
                this.Logger.LogError($"Conversation not found. ChatId {chatId}.");
                throw new Exception($"Conversation not found. ChatId {chatId}.");
            }

            value.Message = message;

            this.Logger.LogInformation($"Feedback saved successfully. ChatId {chatId}");
        }

        public void StopConversation(long chatId)
        {
            this.Conversations.Remove(chatId);

            this.Logger.LogInformation($"Conversation stopped successfully. ChatId {chatId}");
        }

        public void StartUrgentFeedback(long chatId)
        {
            this.Conversations[chatId] = new FeedbackInfo
            {
                Type = FeedbackType.UrgentFeedback
            };

            this.Logger.LogInformation($"Urgent feedback started successfully. ChatId {chatId}");
        }

        public void StartGeneralFeedback(long chatId)
        {
            this.Conversations[chatId] = new FeedbackInfo
            {
                Type = FeedbackType.GeneralFeedback
            };

            this.Logger.LogInformation($"General feedback started successfully. ChatId {chatId}");
        }

        public void StartSubjectFeedback(long chatId, string subjectName)
        {
            this.Conversations[chatId] = new FeedbackInfo
            {
                Type = FeedbackType.SubjectFeedback,
                Subject = subjectName
            };

            this.Logger.LogInformation($"Subject feedback started successfully. ChatId {chatId}, Subject {subjectName}");
        }

        public async Task SendFeedbackAsync(long chatId)
        {
            if (!this.Conversations.TryGetValue(chatId, out var value)
                && value?.Message is null)
            {
                this.Logger.LogError($"Chat not found or no feedback message. ChatId {chatId}");
                throw new Exception($"Chat not found or no feedback message. ChatId {chatId}");
            }

            if (value.Type == FeedbackType.UrgentFeedback)
            {
                await this.SendUrgentFeedback(value.Message);
            }
            else
            {
                await this.FeedbackRepository.AddFeedbackInfoAsync(value);
                this.Logger.LogInformation($"Saved feedback in repository.");
            }

            this.Conversations.Remove(chatId);
        }

        public ConversationStatus GetConversationStatus(long chatId)
        {
            return this.Conversations.TryGetValue(chatId, out var value)
                ? value.Message is null
                    ? ConversationStatus.AwaitingFeedback
                    : ConversationStatus.AwaitingConfirmation
                : ConversationStatus.Default;
        }

        private async Task SendUrgentFeedback(string message)
        {
            var text = $"Алерт! Кто-то оставил срочную обратную связь:\n\n" +
                        $"\"{message}\"";

            await this.BotClient.SendTextMessageAsync(this.Settings.UrgentFeedbackChatId, text);

            this.Logger.LogInformation("Sent urgent feedback successfully.");
        }
    }
}
