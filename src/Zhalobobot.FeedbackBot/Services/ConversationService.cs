﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Api.Services;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback;
using Zhalobobot.Common.Models.Feedback.Requests;
using Zhalobobot.Common.Models.FeedbackChat;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Reply;
using Zhalobobot.Common.Models.Reply.Requests;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;
using Zhalobobot.Common.Models.UserCommon;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Bot.Services
{
    public class ConversationService : IConversationService
    {
        private ITelegramBotClient BotClient { get; }
        private IZhalobobotServices Client { get; }
        private EntitiesCache Cache { get; }
        private ILogger Logger { get; }

        private IDictionary<long, Conversation> Conversations { get; }
            = new ConcurrentDictionary<long, Conversation>();

        public ConversationService(
            ITelegramBotClient botClient,
            IZhalobobotServices client,
            EntitiesCache cache,
            ILogger<ConversationService> logger)
        {
            BotClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Cache = cache;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SaveMessage(long chatId, Message message)
        {
            if (!Conversations.TryGetValue(chatId, out var value))
            {
                Logger.LogError($"Conversation not found. ChatId {chatId}.");
                throw new Exception($"Conversation not found. ChatId {chatId}.");
            }

            value.Messages.Add(message);

            Logger.LogInformation($"Message saved successfully. ChatId {chatId}");
        }

        public void StopConversation(long chatId)
        {
            Conversations.Remove(chatId);

            Logger.LogInformation($"Conversation stopped successfully. ChatId {chatId}");
        }

        public void StartUrgentFeedback(long chatId)
        {
            var student = Cache.Students.Get(chatId);

            Conversations[chatId] = new Conversation
            {
                Feedback = new Feedback(FeedbackType.Urgent, student)
            };

            Logger.LogInformation($"Urgent feedback started successfully. ChatId {chatId}");
        }

        public void StartGeneralFeedback(long chatId)
        {
            var student = Cache.Students.Get(chatId);

            Conversations[chatId] = new Conversation
            {
                Feedback = new Feedback(FeedbackType.General, student)
            };

            Logger.LogInformation($"General feedback started successfully. ChatId {chatId}");
        }

        public void StartSubjectFeedback(long chatId, string subjectName)
        {
            var student = Cache.Students.Get(chatId);

            var subject = Cache.Subjects
                .Get(subjectName)
                .First(s => s.Course == student.Course && s.Semester == SemesterHelper.Current);

            var feedback = new Feedback(FeedbackType.Subject, student, null, subject, new SubjectSurvey());

            Conversations[chatId] = new Conversation
            {
                Feedback = feedback
            };

            Logger.LogInformation($"Subject feedback started successfully. ChatId {chatId}, Subject {subjectName}");
        }

        public void StartAddSpecialCoursesFeedback(long chatId)
        {
            Conversations[chatId] = new Conversation
            {
                SelectedSpecialCourses = new HashSet<string>()
            };
            
            Logger.LogInformation($"Add special courses started successfully. ChatId {chatId}.");
        }

        public async Task SendFeedbackAsync(long chatId, int messageId)
        {
            if (!Conversations.TryGetValue(chatId, out var value))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            var student = Cache.Students.Get(chatId);

            var feedback = value.Feedback with { Student = student };

            await SaveStructuredFeedback(chatId, feedback, messageId).ConfigureAwait(false);

            Conversations.Remove(chatId);
        }

        public ConversationStatus GetConversationStatus(long chatId)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
                return ConversationStatus.Default;

            var feedback = conversation.Feedback;

            if (feedback.Type == FeedbackType.Subject)
            {
                var rating = feedback.SubjectSurvey!.Rating;
                if (rating == 0)
                    return ConversationStatus.AwaitingRating;

                if (rating >= 3 && feedback.SubjectSurvey!.LikedPoints.Count == 0)
                    return ConversationStatus.AwaitingLikedPointsPollAnswer;

                if (rating <= 3 && feedback.SubjectSurvey!.UnlikedPoints.Count == 0)
                    return ConversationStatus.AwaitingUnlikedPointsPollAnswer;

                return ConversationStatus.AwaitingConfirmation;
            }

            return conversation.Messages.Count == 0
                ? ConversationStatus.AwaitingMessage
                : ConversationStatus.AwaitingConfirmation;
        }

        public void SaveRating(long chatId, int rating)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            conversation.Feedback.SubjectSurvey!.Rating = rating;
        }

        public void SavePollInfo(long chatId, PollInfo pollInfo)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            conversation.LastPollInfo = pollInfo;
        }

        public void SaveSelectedSpecialCourses(long chatId, HashSet<string> selectedSpecialCourses)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            conversation.SelectedSpecialCourses = selectedSpecialCourses;
        }

        public PollInfo GetLastPollInfo(long chatId)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            return conversation.LastPollInfo;
        }

        public HashSet<string> GetSelectedSpecialCourses(long chatId)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            return conversation.SelectedSpecialCourses;
        }

        public bool HaveSelectedSpecialCourses(long chatId) => Conversations.TryGetValue(chatId, out _);

        public void ProcessPollAnswer(long chatId, ICollection<string> result, bool isLikedPoints = false)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            if (isLikedPoints)
                conversation.Feedback.SubjectSurvey!.LikedPoints = result;
            else
                conversation.Feedback.SubjectSurvey!.UnlikedPoints = result;
        }

        private async Task SaveStructuredFeedback(long chatId, Feedback feedback, int messageId)
        {
            if (!Conversations.TryGetValue(chatId, out var conversation))
            {
                Logger.LogError($"Chat not found. ChatId {chatId}");
                throw new Exception($"Chat not found. ChatId {chatId}");
            }

            if (conversation.Messages.Count == 0)
            {
                conversation.Messages = new List<Message> { new Message { MessageId = messageId } };
            }
            
            Logger.LogInformation($"Messages: {conversation.Messages.ToPrettyJson()}");

            var firstMessage = true;
            foreach (var message in conversation.Messages)
            {
                Logger.LogInformation($"Start working with {message.ToPrettyJson()}");
                var newFeedback = feedback with { Message = message.Text, MessageId = message.MessageId };
                await Client.Feedback.Add(new AddFeedbackRequest(newFeedback));

                var feedbackChatData = Cache.FeedbackChatData;
                foreach (var feedbackChatInfo in feedbackChatData.All)
                {
                    await ProcessFeedbackChatSending(feedbackChatInfo, newFeedback, chatId, message.Entities, firstMessage);
                }

                firstMessage = false;
            }

            Logger.LogInformation($"Saved feedback in repository. ChatId {chatId}.");
        }

        private async Task ProcessFeedbackChatSending(FeedbackChatData data, Feedback feedback, long chatId, IEnumerable<MessageEntity>? entities, bool firstMessage)
        {
            Logger.LogInformation($"Start processing feedback for chat {data.ChatId}. Feedback {feedback.ToPrettyJson()}. Settings {data.ToPrettyJson()}");

            var message = FormFeedbackMessage(feedback, data);

            if (data.FeedbackTypes.Any() && data.FeedbackTypes.All(x => x != feedback.Type))
            {
                Logger.LogInformation($"Failed FeedbackType Check. ChatId {data.ChatId}");
                return;
            }

            if (data.SubjectNames.Any() && data.SubjectNames.All(x => x != feedback.Subject?.Name))
            {
                Logger.LogInformation($"Failed Subject Check. ChatId {data.ChatId}.");
                return;
            }

            if (data.StudyGroups.Any() && !data.StudyGroups.Any(x =>
                x.Course == feedback.Student.Course
                && x.Group != feedback.Student.Group
                && (!x.Subgroup.HasValue || x.Subgroup != feedback.Student.Subgroup)))
            {
                Logger.LogInformation($"Failed Student Check. ChatId {data.ChatId}.");
                return;
            }

            var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton(BotMessageHelper.StartReplyDialog)
            {
                CallbackData = CallbackDataPrefix.StartReplyDialog
            });

            if (string.IsNullOrWhiteSpace(feedback.Message))
            {
                var sentMessage = await BotClient.SendTextMessageAsync(
                    data.ChatId,
                    message,
                    parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup);

                var reply = new Reply(
                    feedback.Student.Id, feedback.Student.Username,
                    chatId, feedback.MessageId, feedback.Message,
                    sentMessage.Chat.Id, sentMessage.MessageId);

                Cache.Replies.Add(reply);
                await Client.Reply.Add(new AddReplyRequest(reply));
            }
            else
            {
                if (firstMessage)
                {
                    await BotClient.SendTextMessageAsync(
                        data.ChatId,
                        message,
                        parseMode: ParseMode.Html);
                }
            }

            if (!string.IsNullOrWhiteSpace(feedback.Message))
            {
                var sentMessage = await BotClient.SendTextMessageAsync(data.ChatId, feedback.Message, entities: entities, replyMarkup: replyMarkup);
                var reply = new Reply(
                    feedback.Student.Id, feedback.Student.Username,
                    chatId, feedback.MessageId, feedback.Message,
                    sentMessage.Chat.Id, sentMessage.MessageId);

                Cache.Replies.Add(reply);
                await Client.Reply.Add(new AddReplyRequest(reply));
            }
            
            Logger.LogInformation($"Send feedback to chat {data.ChatId} successfully.");
        }

        private string FormFeedbackMessage(Feedback feedback, FeedbackChatData data)
        {
            var builder = new StringBuilder();
            
            builder.Append(FormStudentInfo(feedback.Student, data.IncludeFullStudentInfo));
            
            if (feedback.SubjectSurvey != null)
            {
                builder.AppendLine($"Оценил курс <code>{feedback.Subject!.Name}</code>");
                builder.AppendLine($"На <code>{feedback.SubjectSurvey.Rating}</code> из 5");
                if (feedback.SubjectSurvey.LikedPoints.Any())
                {
                    builder.AppendLine($"Понравилось: <code>{string.Join("; ", feedback.SubjectSurvey.LikedPoints)}</code>");
                }
                if (feedback.SubjectSurvey.UnlikedPoints.Any())
                {
                    builder.AppendLine($"Не понравилось: <code>{string.Join("; ", feedback.SubjectSurvey.UnlikedPoints)}</code>");
                }
                if (!string.IsNullOrWhiteSpace(feedback.Message))
                {
                    builder.AppendLine("И прокомментировал оценку так:");
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(feedback.Message))
                {
                    builder.AppendLine("Оставил сообщение:");
                }
                else
                {
                    builder.AppendLine("Решил ничего не писать");
                }
            }
            
            return builder.ToString();
        }

        private string FormStudentInfo(Student student, bool inludeFullStudentInfo = false)
        {
            var builder = new StringBuilder();
            var course = $"ФТ-{(int)student.Course}0{(int)student.Group}-{(int)student.Subgroup}";

            if (inludeFullStudentInfo)
            {
                builder.AppendLine($"{student.Name ?? Name.UnknownPerson}");
                builder.AppendLine(course);
                if (!string.IsNullOrEmpty(student.Username))
                {
                    builder.AppendLine($"@{student.Username}");
                }
            }
            else
            {
                builder.AppendLine($"Кто-то из <code>{course}</code>");
            }

            return builder.ToString();
        }
    }
}
