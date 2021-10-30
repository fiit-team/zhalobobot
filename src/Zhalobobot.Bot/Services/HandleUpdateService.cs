using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;
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

        private static ReplyKeyboardMarkup DefaultKeyboardMarkup { get; } = new(
            new[]
            {
                new KeyboardButton[] { Buttons.Subjects },
                new KeyboardButton[] { Buttons.GeneralFeedback },
                new KeyboardButton[] { Buttons.Alarm }
            })
        {
            ResizeKeyboard = true
        };

        private static ReplyKeyboardMarkup SubmitKeyboardMarkup { get; } = new (
            new KeyboardButton[]
            {
                Buttons.Submit,
                Buttons.Back
            })
        {
            ResizeKeyboard = true
        };

        private static ReplyKeyboardMarkup CancelKeyboardMarkup { get; } = new(
            new KeyboardButton[]
            {
                Buttons.Back
            })
        {
            ResizeKeyboard = true
        };

        public HandleUpdateService(ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            IConversationService conversationService,
            ILogger<HandleUpdateService> logger)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            ConversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            if (message.Type != MessageType.Text)
                return;

            var userName = $"@{message.From.Username}";

            var student = (await Client.Student.GetAbTestStudent(userName)).Result;

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            var action = message.Text.Trim() switch
            {
                Buttons.Alarm => HandleAlertFeedbackAsync(BotClient, message),
                Buttons.Subjects => SendFeedbackKeyboardAsync(BotClient, message),
                Buttons.GeneralFeedback => HandleGeneralFeedbackAsync(BotClient, message, student),
                Buttons.Submit => SendFeedbackAsync(BotClient, message, student),
                Buttons.Back => CancelFeedbackAsync(BotClient, message),
                _ => conversationStatus == ConversationStatus.Default
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

            const string text = @"Что случилось? Опиши ситуацию подробно, я позову на помощь сразу же, как ты нажмешь кнопку ""Готово""";

            return await BotClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: CancelKeyboardMarkup);
        }

        private async Task<Message> HandleGeneralFeedbackAsync(ITelegramBotClient bot, Message message, AbTestStudent student)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartGeneralFeedback(message.Chat.Id);

            var builder = new StringBuilder();
            builder.AppendLine("Расскажи про всё, что наболело и что понравилось");
            builder.AppendLine();

            if (student.InGroupA)
            {
                builder.AppendLine("• Пиши текстом, я пока не умею обрабатывать голосовые сообщения");
                builder.AppendLine();
                builder.AppendLine("• Пиши пожалуйста одну мысль в одном сообщении, чтобы админу было проще");
            }
            else
            {
                builder.AppendLine("Пиши текстом, я пока не умею обрабатывать голосовые сообщения");
            }

            builder.AppendLine();
            builder.AppendLine(@"Как закончишь — нажимай ""Готово""");

            return await bot.SendTextMessageAsync(
                message.Chat.Id, 
                builder.ToString(),
                replyMarkup: CancelKeyboardMarkup);
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

            ConversationService.SaveMessage(message.Chat.Id, message.Text);
            
            const string text = @"Если больше мыслей не осталось — жми ""Готово""";

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: SubmitKeyboardMarkup);
        }

        private async Task<Message> SendFeedbackAsync(ITelegramBotClient bot, Message message, AbTestStudent student)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            var replyMarkup = DefaultKeyboardMarkup;
            if (conversationStatus == ConversationStatus.Default)
            {
                text = "Ничего на отправку";
            }
            else if (conversationStatus == ConversationStatus.AwaitingFeedback)
            {
                text = "Напиши сообщение";
                replyMarkup = CancelKeyboardMarkup;
            }
            else
            {
                await ConversationService.SendFeedbackAsync(message.Chat.Id, student);
                text = "Спасибо, я всё записал!";
            }

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: replyMarkup);
        }

        private async Task<Message> CancelFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            
            ConversationService.StopConversation(message.Chat.Id);

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                Buttons.MainMenu,
                replyMarkup: DefaultKeyboardMarkup);
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

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            Logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        private Task HandleErrorAsync(Exception exception)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Logger.LogInformation(errorMessage);
            return Task.CompletedTask;
        }

        private static async Task<Message> Usage(ITelegramBotClient bot, Message message)
        {
            var usage = new StringBuilder();

            usage.AppendLine("ALARM — это красная кнопка. Если всё очень плохо — нажми");
            usage.AppendLine();
            usage.AppendLine("Выбрать предмет — поставить оценку от 1 до 5 и оставить комментарий конкретному предмету");
            usage.AppendLine();
            usage.AppendLine("Просто пожаловаться — выскажи всё, что лежит на душе: и хорошее, и плохое");

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                usage.ToString(),
                replyMarkup: DefaultKeyboardMarkup);
        }
    }
}