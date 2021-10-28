using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Services
{
    public class HandleUpdateService
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotApiClient Client { get; }
        private IConversationService ConversationService { get; }
        private ILogger<HandleUpdateService> Logger { get; }
        private Dictionary<string, AbTestStudent> Students { get; }

        public HandleUpdateService(ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            IConversationService conversationService,
            ILogger<HandleUpdateService> logger)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            ConversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Students = new Dictionary<string, AbTestStudent>();
        }

        public async Task EchoAsync(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
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
            Logger.LogInformation($"Receive message type: {message.Type}");

            var userName = $"@{message.From.Username}";
            
            AbTestStudent student;

            if (Students.ContainsKey(userName))
                student = Students[userName];
            else
            {
                student = (await Client.Student.GetAbTestStudent(userName)).Result;
                Students[userName] = student;
            }

            if (message.Type != MessageType.Text)
                return;

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            var action = message.Text.Split(' ').First() switch
            {
                "/alert" => HandleAlertFeedbackAsync(BotClient, message),
                "/list" => SendFeedbackKeyboardAsync(BotClient, message),
                "/general" => HandleGeneralFeedbackAsync(BotClient, message),
                "Отправить" => SendFeedbackAsync(BotClient, message, student),
                "Отменить" => CancelFeedbackAsync(BotClient, message),
                _ => conversationStatus == Models.ConversationStatus.Default
                    ? Usage(BotClient, message)
                    : SaveFeedbackAsync(BotClient, message)
            };

            var sentMessage = await action;
            Logger.LogInformation($"The message was sent with id: {sentMessage.MessageId}");
        }

        private async Task<Message> HandleAlertFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartUrgentFeedback(message.Chat.Id);

            var text = "Ты выбрал срочную обратную связь. Напиши сообщение.";

            return await BotClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task<Message> HandleGeneralFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartGeneralFeedback(message.Chat.Id);

            var text = "Ты выбрал общую обратную связь. Напиши сообщение.";

            return await bot.SendTextMessageAsync(message.Chat.Id, text);
        }

        private async Task<Message> SendFeedbackKeyboardAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var subjects = (await Client.Subject.GetSubjects()).Result;
            
            var inlineKeyboard = new InlineKeyboardMarkup(subjects.Select(subject => new[] { InlineKeyboardButton.WithCallbackData(subject.Name, subject.Name) }));

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Выбери предмет.",
                replyMarkup: inlineKeyboard);
        }

        private async Task<Message> SaveFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.SaveFeedback(message.Chat.Id, message.Text);

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
                message.Chat.Id,
                text,
                replyMarkup: replyKeyboardMarkup);
        }

        private async Task<Message> SendFeedbackAsync(ITelegramBotClient bot, Message message, AbTestStudent student)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
               
            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            if (conversationStatus != Models.ConversationStatus.AwaitingConfirmation)
            {
                text = "Ничего на отправку.";
            }
            else
            {
                await ConversationService.SendFeedbackAsync(message.Chat.Id, student);
                text = "Спасибо за обратную связь! :)";
            }

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task<Message> CancelFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            if (conversationStatus != Models.ConversationStatus.AwaitingConfirmation)
            {
                text = "Ничего на отмену.";
            }
            else
            {
                ConversationService.StopConversation(message.Chat.Id);
                text = "Что ж, может, в другой раз. :(";
            }

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;

            await BotClient.SendChatActionAsync(chatId, ChatAction.Typing);

            ConversationService.StartSubjectFeedback(chatId, callbackQuery.Data);

            await BotClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);

            var text = $"Ты выбрал обратную связь по предмету \"{callbackQuery.Data}\". Напиши сообщение.";

            await BotClient.SendTextMessageAsync(
                chatId,
                text,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            const string usage = "Usage:\n\n" +
                                 "/alert    - Срочная обратная связь\n\n" +
                                 "/list     - Выбрать предмет\n\n" +
                                 "/general  - Общая обратная связь";

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                usage,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            Logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(Exception exception)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Logger.LogInformation(errorMessage);
            return Task.CompletedTask;
        }
    }
}