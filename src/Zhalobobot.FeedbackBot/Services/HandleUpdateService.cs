using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Bot.Models;
using Zhalobobot.Bot.Schedule;
using Zhalobobot.Bot.Services.Handlers;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Exceptions;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.UserCommon;
using Zhalobobot.Common.Models.Reply;
using Emoji = Zhalobobot.Bot.Models.Emoji;
using Zhalobobot.Common.Models.Reply.Requests;

namespace Zhalobobot.Bot.Services
{
    public class HandleUpdateService
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotApiClient Client { get; }
        private IConversationService ConversationService { get; }
        private IPollService PollService { get; }
        private IScheduleMessageService ScheduleMessageService { get; }
        private ILogger<HandleUpdateService> Logger { get; }
        private EntitiesCache Cache { get; }

        private UpdateHandlerAdmin AdminHandler { get; }

        private bool IsFirstYearWeekOdd { get; }

        public HandleUpdateService(ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            IConversationService conversationService,
            IPollService pollService,
            IScheduleMessageService scheduleMessageService,
            EntitiesCache cache,
            ILogger<HandleUpdateService> logger, 
            IConfiguration configuration,
            UpdateHandlerAdmin adminHandler)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            ConversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            PollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            ScheduleMessageService = scheduleMessageService;
            Cache = cache;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            IsFirstYearWeekOdd = bool.Parse(configuration["IsFirstYearWeekOdd"]);
            AdminHandler = adminHandler;
        }

        public async Task EchoAsync(Update update)
        {
            if (AdminHandler.Accept(update))
            {
                try
                {
                    await AdminHandler.HandleUpdate(update);
                    return;
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                }
            }
            
            var chat = update.Message?.Chat;
            if (chat is not null && chat.Type != ChatType.Private)
            {
                // Временно не обрабатываем логику бота в группе.
                return;
            }

            try
            {
                if (await HandleAddStudent())
                    return;
            }
            catch (CacheNotInitializedException)
            {
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

            async Task<bool> HandleAddStudent()
            {
                if (update.Message is {} message)
                {
                    var student = Cache.Students.Find(message.Chat.Id);

                    if (student == null)
                    {
                        if (Cache.StudentData.Find($"@{message.Chat.Username}") is { } data)
                        {
                            student = new Student(message.Chat.Id, message.Chat.Username, data.Course, data.Group, data.Subgroup, data.Name);
                            Cache.Students.Add(student);
                            await Client.Student.Add(new AddStudentRequest(student));
                            await StartUsage(BotClient, message);
                        }
                        else if (update.Type == UpdateType.CallbackQuery)
                            await BotOnCallbackQueryReceived(update.CallbackQuery);
                        else
                            await AddStudent();

                        return true;
                    }
                
                    async Task AddStudent()
                    {
                        await BotClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    
                        await BotClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Привет! Мы с тобой еще не знакомы, так что ответь на пару моих вопросиков :)\nСначала укажи курс:",
                            replyMarkup: Keyboards.AddCourseKeyboard);
                    }
                }

                return false;
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            Logger.LogInformation($"Receive message type: {message.Type}");

            if (message.Type != MessageType.Text)
                return;

            if (await TryHandleReplyMessage(message))
            {
                return;
            }

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            var action = message.Text.Trim() switch
            {
                "/start" => StartUsage(BotClient, message),
                Buttons.Alarm => HandleAlertFeedbackAsync(BotClient, message),
                Buttons.Subjects => HandleSubjectsAsync(BotClient, message),
                Buttons.GeneralFeedback => HandleGeneralFeedbackAsync(BotClient, message),
                Buttons.Schedule => HandleScheduleAsync(BotClient, message),
                Buttons.Submit => SendFeedbackAsync(BotClient, message),
                Buttons.MainMenu => CancelFeedbackAsync(BotClient, message),
                _ => conversationStatus == ConversationStatus.Default
                    ? Usage(BotClient, message)
                    : SaveFeedbackAsync(BotClient, message)
            };

            await action;
            Logger.LogInformation($"The message has been processed. MessageId: {message.MessageId}");
        }

         
        private async Task<bool> TryHandleReplyMessage(Message message)
        {
            var replyToMessage = message.ReplyToMessage;

            if (message.ReplyToMessage is null)
            {
                return false;
            }

            var reply = Cache.Replies.FindBySentMessage(replyToMessage.Chat.Id, replyToMessage.MessageId);
            if (reply is null)
            {
                return false;
            }

            var sentMessage = await BotClient.SendTextMessageAsync(
                reply.ChatId,
                "На твоё сообщение ответили:\n\n" +
                $"{message.Text}\n\n" +
                $"Если хочешь продолжить общение, можешь ответить реплаем на это сообщение.",
                replyToMessageId: reply.MessageId);

            var newReply = new Reply(
                message.From.Id, message.From.Username, message.Chat.Id,
                message.MessageId, message.Text,
                sentMessage.Chat.Id, sentMessage.MessageId);

            await BotClient.SendTextMessageAsync(
                reply.ChildChatId,
                "Сообщение отправлено");

            Cache.Replies.Add(newReply);
            await Client.Reply.Add(new AddReplyRequest(newReply));

            return true;
        }

        private async Task<Message> HandleAlertFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartUrgentFeedback(message.Chat.Id);

            const string text = @"Что случилось? Опиши ситуацию подробно, я позову на помощь сразу же, как ты нажмешь кнопку ""Готово""";

            return await BotClient.SendTextMessageAsync(
                message.Chat.Id,
                text,
                replyMarkup: Keyboards.SubmitKeyboard);
        }

        private async Task<Message> HandleGeneralFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            ConversationService.StartGeneralFeedback(message.Chat.Id);

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                BuildStartFeedbackMessage(),
                replyMarkup: Keyboards.SubmitKeyboard);
        }

        private async Task<Message> HandleSubjectsAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            
            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Выбери категорию",
                replyMarkup: Keyboards.GetSubjectCategoryKeyboard);
        }

        private async Task<Message> HandleScheduleAsync(ITelegramBotClient bot, Message message)
        {
            var student = Cache.Students.Get(message.Chat.Id);

            var lastStudyWeekDay = Cache
                .ActualSchedule(student, DateHelper.CurrentWeekParity(IsFirstYearWeekOdd))
                .LastStudyWeekDay();

            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Выбери нужный вариант",
                replyMarkup: Keyboards.ChooseScheduleDayKeyboard(lastStudyWeekDay));
        }

        private async Task SaveFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            ConversationService.SaveMessage(message.Chat.Id, message);
            
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            const string text = @"Если больше мыслей не осталось — жми ""Готово""";

            await bot.SendTextMessageAsync(
                message.Chat.Id,
                text);
        }

        private async Task SendFeedbackAsync(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            var conversationStatus = ConversationService.GetConversationStatus(message.Chat.Id);

            string text;
            var student = Cache.Students.Get(message.Chat.Id); //todo: убрать после того, как добавим обработку 3го курса
            var replyMarkup = Keyboards.DefaultKeyboard(student);

            if (conversationStatus == ConversationStatus.AwaitingConfirmation)
            {
                await ConversationService.SendFeedbackAsync(message.Chat.Id, message.MessageId);
                text = "Спасибо, я всё записал!";
            }
            else if (conversationStatus == ConversationStatus.AwaitingMessage)
            {
                text = @"Отправь сообщение, а потом нажми ""Готово""";
                replyMarkup = Keyboards.SubmitKeyboard;
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
            var (callbackType, data) = callbackQuery.Data.SplitPair(Strings.Separator);

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
                case CallbackDataPrefix.AddCourse:
                    await HandleAddCourseCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.AddCourseAndGroup:
                    await HandleAddCourseAndGroupCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.AddCourseAndGroupAndSubgroup:
                    await HandleAddCourseAndGroupAndSubgroupCallback(chatId, data, callbackQuery.Message).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.Feedback:
                    await HandleFeedbackCallback(chatId, data, messageId).ConfigureAwait(false);
                    break;
                case CallbackDataPrefix.ChooseScheduleRange:
                    await HandleChooseScheduleRange(chatId, data, messageId).ConfigureAwait(false);
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
            var student = Cache.Students.Get(chatId);
            
            var subjectCategory = Enum.Parse<SubjectCategory>(data);

            var subjects = Cache.Subjects.Get((student.Course, subjectCategory));

            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                "Выбери предмет");
            await BotClient.EditMessageReplyMarkupAsync(
                chatId,
                messageId,
                Keyboards.GetSubjectsKeyboard(subjects));
        }

        private async Task HandleAddCourseCallback(long chatId, string data, int messageId)
        {
            var course = Enum.Parse<Course>(data);
            
            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                $"Окей, твой курс {(int)course}й, теперь давай узнаем группу");
            
            await BotClient.EditMessageReplyMarkupAsync(
                chatId,
                messageId,
                Keyboards.AddCourseAndGroupKeyboard(course));
        }
        
        private async Task HandleAddCourseAndGroupCallback(long chatId, string data, int messageId)
        {
            var items = data.Split(Strings.Separator, 2);
            var course = Enum.Parse<Course>(items[0]);
            var group = Enum.Parse<Group>(items[1]);

            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                $"Окей, твоя группа ФТ-{(int)course}0{(int)group}, теперь давай узнаем полугруппу");
            
            await BotClient.EditMessageReplyMarkupAsync(
                chatId,
                messageId,
                Keyboards.AddCourseAndGroupAndSubgroupKeyboard(course, group));
        }
        
        private async Task HandleAddCourseAndGroupAndSubgroupCallback(long chatId, string data, Message message)
        {
            var items = data.Split(Strings.Separator, 3);
            var course = Enum.Parse<Course>(items[0]);
            var group = Enum.Parse<Group>(items[1]);
            var subgroup = Enum.Parse<Subgroup>(items[2]);

            if (Cache.Students.Find(chatId) is {} student)
            {
                await BotClient.EditMessageTextAsync(
                    chatId,
                    message.MessageId,
                    $"Кажется, что ты уже сохранён с группой ФТ-{(int)student.Course}0{(int)student.Group}-{(int)student.Subgroup}. В будущем добавим возможность редактирования, а пока поживи так)");

                return;
            }

            student = new Student(
                chatId,
                message.Chat.Username,
                course,
                group,
                subgroup,
                new Name(message.Chat.LastName, message.Chat.FirstName, null));

            await Client.Student.Add(new AddStudentRequest(student));
            
            Cache.Students.Add(student);

            await BotClient.EditMessageTextAsync(
                chatId,
                message.MessageId,
                $"Спасибо, записал тебе группу ФТ-{(int)course}0{(int)group}-{(int)subgroup}. Теперь можешь свободно пользоваться ботом!");
            
            await StartUsage(BotClient, message);
        }

        private async Task HandleFeedbackCallback(long chatId, string data, int messageId)
        {
            await BotClient.DeleteMessageAsync(chatId, messageId);

            await StartSubjectFeedback(chatId, data);
        }

        private async Task HandleSubjectCallback(long chatId, string data, int messageId)
        {
            var student = Cache.Students.Get(chatId);

            var subject = Cache.Subjects
                .Get(student.Course)
                .First(s => s.Name.GetHashCode().ToString() == data);

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

        public async Task HandleChooseScheduleRange(long chatId, string data, int messageId)
        {
            var student = Cache.Students.Get(chatId);
            
            var scheduleDay = (ScheduleDay)int.Parse(data);
            
            var actualSchedule = Cache
                .ActualSchedule(
                    student, 
                    scheduleDay.IsCurrentWeek() 
                        ? DateHelper.CurrentWeekParity(IsFirstYearWeekOdd) 
                        : DateHelper.NextWeekParity(IsFirstYearWeekOdd)
                 )
                .ToArray();
            
            var formattedMessage = ScheduleMessageFormatter.Format(actualSchedule, scheduleDay, out var whenDelete);

            if (whenDelete.HasValue)
                ScheduleMessageService.AddMessageToUpdate(chatId, (data, messageId, whenDelete.Value));

            await BotClient.EditMessageTextAsync(
                chatId,
                messageId,
                $"<pre>{formattedMessage}</pre>", 
                ParseMode.Html);
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
                replyMarkup: Keyboards.RatingKeyboard);
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
                await BotClient.SendTextMessageAsync(
                    chatId,
                    BuildStartFeedbackMessage(true),
                    replyMarkup: Keyboards.SubmitKeyboard);
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

        private static string BuildStartFeedbackMessage(bool isSubjectFeedback = false)
        {
            var builder = new StringBuilder();
            builder.AppendLine(isSubjectFeedback
                ? "Расскажи подробнее, что думаешь о курсе?"
                : "Расскажи про всё, что наболело и что понравилось");
            builder.AppendLine();
            builder.AppendLine("• Пиши текстом, я пока не умею обрабатывать голосовые сообщения");
            builder.AppendLine();
            builder.AppendLine("• Пиши пожалуйста одну мысль в одном сообщении, чтобы админу было проще");
            builder.AppendLine();
            builder.AppendLine(isSubjectFeedback
                ? @"Если ничего не хочешь сказать — жми ""Готово"""
                : @"Как закончишь — нажимай ""Готово""");

            return builder.ToString();
        }

        private async Task<Message> StartUsage(ITelegramBotClient bot, Message message)
        {
            var student = Cache.Students.Get(message.Chat.Id);
            var usage = new StringBuilder();

            usage.AppendLine($"{Buttons.Subjects} — поставить оценку от 1 до 5 и оставить комментарий конкретному предмету");
            usage.AppendLine();
            usage.AppendLine($"{Buttons.GeneralFeedback} — выскажи всё, что лежит на душе: и хорошее, и плохое");
            usage.AppendLine();
            usage.AppendLine($"{Buttons.Alarm} — это красная кнопка. Если всё очень плохо — нажми");
            
            // todo: вернуть для второго курса
            if (student.Course < Course.Second) //todo: убрать после того, как добавим обработку 3го курса
            {
                usage.AppendLine();
                usage.AppendLine($"{Buttons.Schedule} — узнать, в каком кабинете следующая пара и какую домашку делать на завтра");
            }

            return await bot.SendTextMessageAsync(
                message.Chat.Id,
                usage.ToString(),
                replyMarkup: Keyboards.DefaultKeyboard(student));
        }

        private async Task Usage(ITelegramBotClient bot, Message message)
        {
            await bot.SendTextMessageAsync(
                message.Chat.Id,
                "Попробуй нажать на кнопку");

            await StartUsage(bot, message);
        }
    }
}