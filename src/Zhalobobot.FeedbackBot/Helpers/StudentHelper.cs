using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Services;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Bot.Helpers;

internal static class StudentHelper
{
    public static async Task<bool> HaveEnoughDataToUseBot(
        Update update,
        ITelegramBotClient botClient,
        IZhalobobotApiClient client,
        EntitiesCache cache,
        IConversationService conversationService,
        Func<CallbackQuery, Task> botOnCallbackQueryReceived,
        Func<ITelegramBotClient, long, Task<Message>> startUsage)
    {
        if (update.Message is { } message)
        {
            var chatId = message.Chat.Id;
            var student = cache.Students.Find(chatId);
            if (student == null)
            {
                if (update.Type == UpdateType.CallbackQuery)
                    await botOnCallbackQueryReceived(update.CallbackQuery);
                else if (cache.StudentData.Find($"@{message.Chat.Username}") is { } data)
                {
                    var specialCourses = conversationService.HaveSelectedSpecialCourses(message.Chat.Id)
                        ? conversationService.GetSelectedSpecialCourses(message.Chat.Id).ToArray()
                        : Array.Empty<string>();
                    
                    student = new Student(message.Chat.Id, message.Chat.Username, data.Course, data.Group, data.Subgroup, data.Name, specialCourses);
                    cache.Students.Add(student);
                    await client.Student.Add(new AddStudentRequest(student));
                    
                    // todo: обработать случай, когда у студента могут быть другие курсы помимо спецкурсов
                    if (data.Course > Course.Second)
                        return await HandleAddSpecialCourses(botClient, chatId, data.Course, cache, conversationService);
                    
                    await startUsage(botClient, message.Chat.Id);
                    return true;
                }
                else
                {
                    await HandleAddStudent(botClient, chatId);
                    return false;
                }

                return true;
            }

            if (student.Course > Course.Second && !student.SpecialCourseNames.Any())
            {
                if (update.Type == UpdateType.CallbackQuery)
                {
                    await botOnCallbackQueryReceived(update.CallbackQuery);
                    return false;
                }
                
                return await HandleAddSpecialCourses(botClient, chatId, student.Course, cache, conversationService);
            }

            return true;
        }

        if (update.Type == UpdateType.CallbackQuery)
            await botOnCallbackQueryReceived(update.CallbackQuery);

        return false;
    }

    public static async Task<bool> HandleAddSpecialCourses(ITelegramBotClient botClient, long chatId, Course course, EntitiesCache cache, IConversationService conversationService)
    {
        conversationService.StartAddSpecialCoursesFeedback(chatId);
        var subjects = cache.Subjects.Get(course).Select(s => s.Name).ToArray();

        await botClient.SendTextMessageAsync(chatId, "Выбери свои спецкурсы:",
            replyMarkup: SelectSpecialCoursesButtonsBuilder.Build(subjects, 4, 0));
        
        return false;
    }
    
    private static async Task HandleAddStudent(ITelegramBotClient botClient, long chatId)
    {
        await botClient.SendChatActionAsync(chatId, ChatAction.Typing);
    
        await botClient.SendTextMessageAsync(
            chatId,
            "Привет! Мы с тобой еще не знакомы, так что ответь на пару моих вопросиков :)\nСначала укажи курс:",
            replyMarkup: Keyboards.AddCourseKeyboard);
    }
}