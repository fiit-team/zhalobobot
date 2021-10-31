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
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;
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
                Buttons.MainMenu
            })
        {
            ResizeKeyboard = true
        };

        private static ReplyKeyboardMarkup CancelKeyboardMarkup { get; } = new(
            new KeyboardButton[]
            {
                Buttons.MainMenu
            })
        {
            ResizeKeyboard = true
        };

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

            var student = (await Client.Student.GetAbTestStudent(userName)).Result;

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            var action = message.Text.Trim() switch
            {
                Buttons.Alarm => HandleAlertFeedbackAsync(BotClient, message),
                Buttons.Subjects => SendSubjectListAsync(BotClient, message),
                Buttons.GeneralFeedback => HandleGeneralFeedbackAsync(BotClient, message, student),
                Buttons.Submit => SendFeedbackAsync(BotClient, message, student),
                Buttons.MainMenu => CancelFeedbackAsync(BotClient, message),
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

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                BuildStartFeedbackMessage(student.InGroupA),
                replyMarkup: CancelKeyboardMarkup);
        }

        private async Task<Message> SendSubjectListAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var subjects = (await Client.Subject.GetSubjects()).Result;

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Выбери предмет",
                replyMarkup: GetSubjectsKeyboard(subjects));
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
            if (conversationStatus == ConversationStatus.AwaitingConfirmation)
            {
                await ConversationService.SendFeedbackAsync(message.Chat.Id, student);
                text = "Спасибо, я всё записал!";
            }
            else
            {
                text = "Прости, не понял. Попробуй нажать на кнопку";
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

            var subjects = (await Client.Subject.GetSubjects()).Result;

            if (callbackQuery.Data == Strings.Skip)
                return;

            var categories = Enum.GetValues<SubjectCategory>()
                .Where(c => c.ToString() == callbackQuery.Data)
                .ToList();

            if (categories.Count == 1)
            {
                await BotClient.EditMessageReplyMarkupAsync(
                    chatId,
                    callbackQuery.Message.MessageId,
                    GetSubjectsKeyboard(subjects, categories.First()));
                return;
            }

            await BotClient.DeleteMessageAsync(chatId, callbackQuery.Message.MessageId);

            var subject = subjects.FirstOrDefault(
                s => s.Name.GetHashCode().ToString() == callbackQuery.Data);
            if (subject != null)
            {
                await StartSubjectFeedback(chatId, subject.Name);
                return;
            }

            var status = ConversationService.GetConversationStatus(chatId);
            if (status != ConversationStatus.AwaitingRating)
                return;

            ConversationService.SaveRating(chatId, int.Parse(callbackQuery.Data));

            await StartPoll(chatId, true);
        }

        private async Task StartSubjectFeedback(long chatId, string subject)
        {
            ConversationService.StartSubjectFeedback(chatId, subject);

            await BotClient.SendTextMessageAsync(
                chatId,
                $"Ты выбрал {subject}",
                replyMarkup: CancelKeyboardMarkup);

            const string text = $"Оцени курс от 1 до 5 {Emoji.Star}";

            var inlineKeyboard = new InlineKeyboardMarkup(Enumerable.Range(1, 5)
                .Select(i => new[] { InlineKeyboardButton.WithCallbackData(
                    string.Join(" ", Enumerable.Repeat(Emoji.Star, i)), i.ToString()) }));

            await BotClient.SendTextMessageAsync(
                chatId,
                text,
                replyMarkup: inlineKeyboard);
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
                await StartPoll(chatId);
            }

            if (status is ConversationStatus.AwaitingUnlikedPointsPollAnswer)
            {
                var pollResult = PollService.GetUnlikedPoints(pollAnswer.OptionIds);
                ConversationService.ProcessPollAnswer(chatId, pollResult);

                var student = (await Client.Student.GetAbTestStudent($"@{pollAnswer.User.Username}")).Result;
                await BotClient.SendTextMessageAsync(
                    chatId,
                    BuildStartFeedbackMessage(student.InGroupA, true),
                    replyMarkup: SubmitKeyboardMarkup);
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
            var categories = Enum.GetValues<SubjectCategory>()
                .Select(c => (
                    (c == category ? Emoji.Arrow : string.Empty) + c.AsString(EnumFormat.Description),
                    c == category ? Strings.Skip : c.ToString()))
                .Select(c => InlineKeyboardButton.WithCallbackData(c.Item1, c.Item2))
                .ToList();

            var inlineKeyboard = new InlineKeyboardMarkup(
                subjects
                    .Where(subject => subject.Category! == category)
                    .Select(subject => new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            subject.Name.Slice(),
                            subject.Name.GetHashCode().ToString())
                    })
                    .Append(categories.Take((categories.Count + 1) / 2))
                    .Append(categories.Skip((categories.Count + 1) / 2)));

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