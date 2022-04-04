using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Reply;
using Zhalobobot.Common.Models.Reply.Requests;

namespace Zhalobobot.Bot.Services.Handlers
{
    public class UpdateHandlerAdmin : IUpdateHandler
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotApiClient Client { get; }
        private IConversationService ConversationService { get; }
        private IPollService PollService { get; }
        private IScheduleMessageService ScheduleMessageService { get; }
        private ILogger<UpdateHandlerAdmin> Logger { get; }
        private EntitiesCache Cache { get; }
        private Settings.Settings Settings { get; }

        public UpdateHandlerAdmin(
            ITelegramBotClient botClient,
            IZhalobobotApiClient client,
            IConversationService conversationService,
            IPollService pollService,
            IScheduleMessageService scheduleMessageService,
            EntitiesCache cache,
            ILogger<UpdateHandlerAdmin> logger,
            Settings.Settings settings)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            ConversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            PollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            ScheduleMessageService = scheduleMessageService;
            Cache = cache;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool Accept(Update update)
        {
            Chat? chat = null;

            if (update.Type == UpdateType.Message)
            {
                chat = update.Message.Chat;
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                chat = update.CallbackQuery.Message.Chat;
            }

            return Cache.FeedbackChatData.All.Any(x => x.ChatId == chat?.Id);
        }

        public async Task HandleUpdate(Update update)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                throw;
            }
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (message.Type != MessageType.Text)
            {
                return;
            }

            var replyToMessage = message.ReplyToMessage;

            if (message.ReplyToMessage is null)
            {
                return;
            }

            var reply = Cache.Replies.FindBySentMessage(replyToMessage.Chat.Id, replyToMessage.MessageId);

            if (reply is null)
            {
                return;
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
        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == "alert-engaged")
            {
                return;
            }

            await BotClient.EditMessageReplyMarkupAsync(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton()
                {
                    Text = $"Занял {callbackQuery.From.Username}",
                    CallbackData = "alert-engaged"
                }));
        }

        private Task UnknownUpdateHandlerAsync(Update update)
        {
            Logger.LogInformation($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
