using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Zhalobobot.Bot.Services
{
    public class HandleUpdateService
    {
        private ITelegramBotClient BotClient { get; }
        private ISubjectsService SubjectsService { get; }
        private IConversationService ConversationService { get; }
        private ILogger<HandleUpdateService> Logger { get; }

        public HandleUpdateService(ITelegramBotClient botClient,
            ISubjectsService subjectsService,
            IConversationService conversationService,
            ILogger<HandleUpdateService> logger)
        {
            this.BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            this.SubjectsService = subjectsService ?? throw new ArgumentNullException(nameof(subjectsService));
            this.ConversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task EchoAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => this.BotOnMessageReceived(update.Message),
                //UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(exception);
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            this.Logger.LogInformation($"Receive message type: {message.Type}");

            if (message.Type != MessageType.Text)
                return;

            var conversationStatus = this.ConversationService.GetConversationStatus(message.Chat.Id);

            var action = message.Text.Split(' ').First() switch
            {
                "/alert" => this.HandleAlertFeedbackAsync(this.BotClient, message),
                "/list" => this.SendFeedbackKeyboardAsync(this.BotClient, message),
                "/general" => this.HandleGeneralFeedbackAsync(this.BotClient, message),
                "Отправить" => this.SendFeedbackAsync(this.BotClient, message),
                "Отменить" => this.CancelFeedbackAsync(this.BotClient, message),
                _ => conversationStatus == Models.ConversationStatus.Default
                    ? Usage(this.BotClient, message)
                    : this.SaveFeedbackAsync(this.BotClient, message)
            };

            var sentMessage = await action;
            this.Logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
        }

        private async Task<Message> HandleAlertFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            this.ConversationService.StartUrgentFeedback(message.Chat.Id);

            var text = "Ты выбрал срочную обратную связь. Напиши сообщение.";

            return await this.BotClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task<Message> HandleGeneralFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            this.ConversationService.StartGeneralFeedback(message.Chat.Id);

            var text = "Ты выбрал общую обратную связь. Напиши сообщение.";

            return await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: text);
        }

        private async Task<Message> SendFeedbackKeyboardAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var inlineKeyboard = new InlineKeyboardMarkup(this.SubjectsService.GetSubjects()
                .Select(subject => new[] { InlineKeyboardButton.WithCallbackData(subject.Name, subject.Name) }));

            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Выбери предмет.",
                replyMarkup: inlineKeyboard);
        }

        private async Task<Message> SaveFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            this.ConversationService.SaveFeedback(message.Chat.Id, message.Text);

            var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton("Отправить"),
                    new KeyboardButton("Отменить")
                })
                {
                    ResizeKeyboard = true
                };

            var text = "Сообщение успешно сохранено.\n" + 
                       "Что мне с ним сделать?\n\n" +
                       "Ты также можешь написать новое сообщение.";
            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: replyKeyboardMarkup);
        }

        private async Task<Message> SendFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
               
            var conversationStatus = this.ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            if (conversationStatus != Models.ConversationStatus.AwaitingConfirmation)
            {
                text = "Ничего на отправку.";
            }
            else
            {
                await this.ConversationService.SendFeedbackAsync(message.Chat.Id);
                text = "Спасибо за обратную связь! :)";
            }

            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task<Message> CancelFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var conversationStatus = this.ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            if (conversationStatus != Models.ConversationStatus.AwaitingConfirmation)
            {
                text = "Ничего на отмену.";
            }
            else
            {
                this.ConversationService.StopConversation(message.Chat.Id);
                text = "Что ж, может, в другой раз. :(";
            }

            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            await this.BotClient.SendChatActionAsync(chatId, ChatAction.Typing);

            this.ConversationService.StartSubjectFeedback(chatId, callbackQuery.Data);

            await this.BotClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);

            var text = $"Ты выбрал обратную связь по предмету \"{callbackQuery.Data}\". Напиши сообщение.";

            await this.BotClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            const string usage = "Usage:\n\n" +
                                 "/alert    - Срочная обратная связь\n\n" +
                                 "/list     - Выбрать предмет\n\n" +
                                 "/general  - Общая обратная связь";

            return await bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: usage,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            this.Logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            this.Logger.LogInformation(errorMessage);
            return Task.CompletedTask;
        }
    }
}