using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumsNET;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.Subject.Requests;
using Emoji = Zhalobobot.Bot.Models.Emoji;

namespace Zhalobobot.Bot.Services
{
    public class HandleUpdateService
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotApiClient Client { get; }
        private IConversationService ConversationService { get; }
        private IPollService PollService { get; }
        private ILogger<HandleUpdateService> Logger { get; }

        public HandleUpdateService(ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            IConversationService conversationService,
            IPollService pollService,
            ILogger<HandleUpdateService> logger)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            ConversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            PollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task EchoAsync(Update update)
        {
            var chat = update.Message?.Chat;
            if (chat is not null && chat.Type != ChatType.Private)
            {
                // Временно не обрабатываем логику бота в группе.
                return;
            }

            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                UpdateType.PollAnswer => BotOnPollAnswerReceived(update.PollAnswer),
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
            
            var request = new GetAbTestStudentRequest { Username = userName };

            var student = (await Client.Student.GetAbTestStudent(request)).Result;

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            var action = message.Text.Trim() switch
            {
                "/start" => StartUsage(BotClient, message),
                Buttons.Alarm => HandleAlertFeedbackAsync(BotClient, message),
                Buttons.Subjects => SendSubjectCategoryKeyboard(BotClient, message),
                Buttons.GeneralFeedback => HandleGeneralFeedbackAsync(BotClient, message, student),
                Buttons.Submit => SendFeedbackAsync(BotClient, message, student),
                Buttons.MainMenu => CancelFeedbackAsync(BotClient, message),
                _ => conversationStatus == ConversationStatus.Default
                    ? Usage(BotClient, message)
                    : SaveFeedbackAsync(BotClient, message, student)
            };

            await action;
            Logger.LogInformation($"The message has been processed. MessageId: {message.MessageId}");
        }

        private async Task<Message> HandleAlertFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartUrgentFeedback(message.Chat.Id);

            const string text = @"Что случилось? Опиши ситуацию подробно, я позову на помощь сразу же, как ты нажмешь кнопку ""Готово""";

            return await BotClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: WellKnownKeyboards.SubmitKeyboard);
        }

        private async Task<Message> HandleGeneralFeedbackAsync(ITelegramBotClient bot, Message message, AbTestStudent student)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartGeneralFeedback(message.Chat.Id);

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                BuildStartFeedbackMessage(student.InGroupA),
                replyMarkup: WellKnownKeyboards.SubmitKeyboard);
        }

        private async Task<Message> SendSubjectCategoryKeyboard(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            
            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Выбери курс",
                replyMarkup: WellKnownKeyboards.ChooseCourseKeyboard);
        }

        private async Task SaveFeedbackAsync(ITelegramBotClient bot, Message message, AbTestStudent student)
        {
            ConversationService.SaveMessage(message.Chat.Id, message.Text);

            if (student.InGroupA)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                const string text = @"Если больше мыслей не осталось — жми ""Готово""";

                await bot.SendTextMessageAsync(
                    message.Chat.Id,
                    text);
            }
        }

        private async Task SendFeedbackAsync(ITelegramBotClient bot, Message message, AbTestStudent student)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            var replyMarkup = WellKnownKeyboards.DefaultKeyboard;

            if (conversationStatus == ConversationStatus.AwaitingConfirmation)
            {
                await ConversationService.SendFeedbackAsync(message.Chat.Id, student);
                text = "Спасибо, я всё записал!";
            }
            else if (conversationStatus == ConversationStatus.AwaitingMessage)
            {
                text = @"Отправь сообщение, а потом нажми ""Готово""";
                replyMarkup = WellKnownKeyboards.SubmitKeyboard;
            }
            else
            {
                await Usage(bot, message);
                return;
            }

            await bot.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: replyMarkup);
        }

        private async Task CancelFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            
            ConversationService.StopConversation(message.Chat.Id);

            await StartUsage(bot, message);
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;
            var tokens = callbackQuery.Data.Split('-', 2);

            var callbackType = tokens[0];
            var data = tokens[1];

            await BotClient.SendChatActionAsync(chatId, ChatAction.Typing);
            
            switch (callbackType)
            {
                case CallbackDataPrefix.SubjectCategory:
                    await HandleSubjectCategoryCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.Subject:
                    await HandleSubjectCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.Rating:
                    await HandleRatingCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.Course:
                    await HandleCourseCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.Feedback:
                    await HandleFeedbackCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                default:
                {
                    var message = $"Unknown callbackType: {callbackType}";
                    Logger.LogError(message);
                    throw new Exception(message);
                }
            }
        }

        private async Task HandleSubjectCategoryCallback(long chatId, string data, int messageId)
        {
            var course = ConversationService.GetCourse(chatId) ?? throw new Exception();
            
            var subjects = await Client.Subject.Get(new GetSubjectsRequest {Course = course}).GetResult();

            var category = Enum.GetValues<SubjectCategory>()
                .First(c => c.ToString() == data);

            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                "Выбери предмет");
            await BotClient.EditMessageReplyMarkupAsync(
                chatId,
                messageId,
                GetSubjectsKeyboard(subjects, category));
        }

        private async Task HandleCourseCallback(long chatId, string data, int messageId)
        {
            var course = Enum.Parse<Course>(data);

            ConversationService.AddOrUpdateCourse(chatId, course);
            
            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                "Выбери категорию");
            
            await BotClient.EditMessageReplyMarkupAsync(
                chatId,
                messageId,
                WellKnownKeyboards.SubjectCategoryKeyboard);
        }

        private async Task HandleFeedbackCallback(long chatId, string data, int messageId)
        {
            ConversationService.StartSubjectFeedback(chatId, data);

            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                "Я тебя внимательно слушаю");

            await BotClient.EditMessageReplyMarkupAsync(chatId, messageId, null);
        }

        private async Task HandleSubjectCallback(long chatId, string data, int messageId)
        {
            if (data == Strings.Back)
            {
                await BotClient.EditMessageTextAsync(
                    chatId,
                    messageId,
                    "Выбери категорию");
                await BotClient.EditMessageReplyMarkupAsync(
                    chatId,
                    messageId,
                    WellKnownKeyboards.SubjectCategoryKeyboard);
                return;
            }

            var course = ConversationService.GetCourse(chatId) ?? throw new Exception();

            var subjects = await Client.Subject.Get(new GetSubjectsRequest{Course = course}).GetResult();

            var subject = subjects.First(
                s => s.Name.GetHashCode().ToString() == data);

            await BotClient.DeleteMessageAsync(chatId, messageId);

            await StartSubjectFeedback(chatId, subject.Name);
        }

        private async Task HandleRatingCallback(long chatId, string data, int messageId)
        {
            await BotClient.DeleteMessageAsync(chatId, messageId);

            var status = ConversationService.GetConversationStatus(chatId);
            if (status != ConversationStatus.AwaitingRating)
                return;

            ConversationService.SaveRating(chatId, int.Parse(data));

            status = ConversationService.GetConversationStatus(chatId);
            await StartPoll(chatId, status == ConversationStatus.AwaitingLikedPointsPollAnswer);
        }

        private async Task StartSubjectFeedback(long chatId, string subject)
        {
            ConversationService.StartSubjectFeedback(chatId, subject);

            await BotClient.SendTextMessageAsync(
                chatId,
                $"Ты выбрал {subject}");

            const string text = $"Оцени курс от 1 до 5 {Emoji.Star}";

            await BotClient.SendTextMessageAsync(
                chatId,
                text,
                replyMarkup: WellKnownKeyboards.RatingKeyboard);
        }

        private async Task BotOnPollAnswerReceived(PollAnswer pollAnswer)
        {
            if (pollAnswer.OptionIds.Length == 0)
                return;

            var chatId = pollAnswer.User.Id;

            var lastPollInfo = ConversationService.GetLastPollInfo(chatId);

            if (lastPollInfo is null || pollAnswer.PollId != lastPollInfo.PollId)
                return;

            await BotClient.StopPollAsync(chatId, lastPollInfo.MessageId);
            await BotClient.DeleteMessageAsync(chatId, lastPollInfo.MessageId);

            var status = ConversationService.GetConversationStatus(chatId);

            if (status is ConversationStatus.AwaitingLikedPointsPollAnswer)
            {
                var pollResult = PollService.GetLikedPoints(pollAnswer.OptionIds);
                ConversationService.ProcessPollAnswer(chatId, pollResult, true);
            }

            if (status is ConversationStatus.AwaitingUnlikedPointsPollAnswer)
            {
                var pollResult = PollService.GetUnlikedPoints(pollAnswer.OptionIds);
                ConversationService.ProcessPollAnswer(chatId, pollResult);
                
                // var student = await Client.Student.GetAbTestStudent(
                //     new GetAbTestStudentRequest { Username = $"@{pollAnswer.User.Username}" }).GetResult();
                //
            }

            status = ConversationService.GetConversationStatus(chatId);
            if (status is ConversationStatus.AwaitingUnlikedPointsPollAnswer)
            {
                await StartPoll(chatId);
            }

            if (status is ConversationStatus.AwaitingConfirmation)
            {
                var student = await Client.Student.GetAbTestStudent(new GetAbTestStudentRequest { Username = $"@{pollAnswer.User.Username}" }).GetResult();
                await BotClient.SendTextMessageAsync(
                    chatId,
                    BuildStartFeedbackMessage(student.InGroupA, true),
                    replyMarkup: WellKnownKeyboards.SubmitKeyboard);
            }
        }

        private async Task StartPoll(long chatId, bool isLikedPointsPoll = false)
        {
            var message = await BotClient.SendPollAsync(
                chatId,
                isLikedPointsPoll ? "Что понравилось?" : "Что не понравилось?",
                isLikedPointsPoll ? PollService.GetLikedPointsPoll() : PollService.GetUnlikedPointsPoll(),
                false,
                allowsMultipleAnswers: true,
                allowSendingWithoutReply: true);

            ConversationService.SavePollInfo(
                chatId,
                new PollInfo(message.Poll.Id, message.MessageId));
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

        private static InlineKeyboardMarkup GetSubjectsKeyboard(IEnumerable<Subject> subjects, SubjectCategory category = 0)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                subjects
                    .Where(subject => subject.Category! == category)
                    .Select(subject => new[]
                        {
                            InlineKeyboardButton.WithCallbackData(
                                subject.Name.Slice(),
                                $"{CallbackDataPrefix.Subject}-{subject.Name.GetHashCode()}")
                        })
                    .Append(new[] 
                        { 
                            InlineKeyboardButton.WithCallbackData(
                                Buttons.Back,
                                $"{CallbackDataPrefix.Subject}-{Strings.Back}")
                        }));

            return inlineKeyboard;
        }

        private static string BuildStartFeedbackMessage(bool isGroupA, bool isSubjectFeedback = false)
        {
            var builder = new StringBuilder();
            builder.AppendLine(isSubjectFeedback
                ? "Расскажи подробнее, что думаешь о курсе?"
                : "Расскажи про всё, что наболело и что понравилось");

            builder.AppendLine();

            if (isGroupA)
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
            builder.AppendLine(isSubjectFeedback
                ? @"Если ничего не хочешь сказать — жми ""Готово"""
                : @"Как закончишь — нажимай ""Готово""");

            return builder.ToString();
        }

        private static async Task<Message> StartUsage(ITelegramBotClient bot, Message message)
        {
            var usage = new StringBuilder();

            usage.AppendLine($"{Buttons.Subjects} — поставить оценку от 1 до 5 и оставить комментарий конкретному предмету");
            usage.AppendLine();
            usage.AppendLine($"{Buttons.GeneralFeedback} — выскажи всё, что лежит на душе: и хорошее, и плохое");
            usage.AppendLine();
            usage.AppendLine($"{Buttons.Alarm} — это красная кнопка. Если всё очень плохо — нажми");

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                usage.ToString(),
                replyMarkup: WellKnownKeyboards.DefaultKeyboard);
        }

        private static async Task Usage(ITelegramBotClient bot, Message message)
        {
            await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Попробуй нажать на кнопку");

            await StartUsage(bot, message);
        }
    }
}