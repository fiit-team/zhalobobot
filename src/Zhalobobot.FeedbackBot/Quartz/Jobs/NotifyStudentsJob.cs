// using System;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using Quartz;
// using Telegram.Bot;
// using Telegram.Bot.Exceptions;
// using Vostok.Hosting.Abstractions;
// using Vostok.Hosting.Abstractions.Requirements;
// using Zhalobobot.Bot.Cache;
// using Zhalobobot.Bot.Helpers;
// using Zhalobobot.Bot.Models.Exceptions;
// using Zhalobobot.Bot.Settings;
// using Zhalobobot.Common.Models.Commons;
// using Zhalobobot.Common.Models.Helpers;
// using Zhalobobot.Common.Models.Serialization;
// using Zhalobobot.Common.Models.Student;
// using Zhalobobot.TelegramMessageQueue;
// using Zhalobobot.TelegramMessageQueue.Exceptions;
//
// namespace Zhalobobot.Bot.Quartz.Jobs
// {
//     [DisallowConcurrentExecution]
//     [RequiresSecretConfiguration(typeof(BotSecrets))]
//     public class NotifyStudentsJob : IJob
//     {
//         private MessageSender MessageSender { get; }
//         private ITelegramBotClient BotClient { get; }
//         private EntitiesCache Cache { get; }
//         private ILogger<NotifyStudentsJob> Log { get; }
//         private BotSecrets Secrets { get; }
//
//         public NotifyStudentsJob(
//             MessageSender messageSender, 
//             ITelegramBotClient botClient, 
//             EntitiesCache cache, 
//             IVostokHostingEnvironment environment,
//             ILogger<NotifyStudentsJob> log)
//         {
//             MessageSender = messageSender;
//             BotClient = botClient;
//             Cache = cache;
//             Log = log;
//             Secrets = environment.SecretConfigurationProvider.Get<BotSecrets>();
//         }
//
//         public async Task Execute(IJobExecutionContext context)
//         {
//             Log.LogInformation($"Time: {DateHelper.EkbTime}");
//             var courses = Cache.ActualSchedule(skipEndTimeCheck: false).ToArray();
//
//             Log.LogInformation($"Select {courses.Length} courses to notify");
//
//             foreach (var course in courses)
//             {
//                 await SendNotifyMessageToStudents(
//                     course.Subject.Course,
//                     course.Group,
//                     course.Subgroup,
//                     course.Subject.Name,
//                     course.Subject.StudentsToNotifyPercent);
//             }
//
//             // return Task.CompletedTask;
//         }
//
//         private async Task SendNotifyMessageToStudents(Course course, Group group, Subgroup subgroup, string subjectName, int studentsPercentToNotify)
//         {
//             Log.LogInformation($"Course: {course.ToPrettyJson()}, Group: {group.ToPrettyJson()}, Subgroup: {subgroup.ToPrettyJson()}");
//
//             var studentsToNotify = GetStudentsToNotify(course, group, subgroup, subjectName, studentsPercentToNotify);
//
//             Log.LogInformation($"Selected students: {studentsToNotify.Select(s => s.Username).ToPrettyJson()}");
//
//             var message = string.Join("\n",
//                 $"Привет! Я догадываюсь, что у тебя закончилась пара по предмету {subjectName}.",
//                 "Пожалуйста, оставь обратную связь по нему :)");
//
//             var studentToIgnoreId = Secrets.StudentIdToIgnoreWhenDesign;
//             
//             foreach (var student in studentsToNotify)
//             {
//                 if (subjectName == "Дизайн" && studentToIgnoreId != null && student.Id == long.Parse(studentToIgnoreId))
//                 {
//                     Log.LogInformation("Skip notify for student.");
//                     continue;
//                 }
//                 if (studentToIgnoreId != null && student.Id == long.Parse(studentToIgnoreId))
//                 {
//                     Log.LogInformation("Can't parse design..");
//                     continue;
//                 }
//                 
//                 while (true)
//                 {
//                     try
//                     {
//                         Log.LogInformation($"Try notify student {student.Id}.");
//                         await BotClient.SendTextMessageAsync(
//                             student.Id,
//                             message,
//                             replyMarkup: Keyboards.SendFeedbackKeyboard(subjectName));
//
//                         Log.LogInformation($"Successfuly notified student. StudentId {student.Id}");
//                         break;
//                     }
//                     catch (ChatNotFoundException e)
//                     {
//                         // skip
//                         Log.LogError($"Skip notify student due to ChatNotFoundException. Error {e.ToPrettyJson()}");
//                         break;
//                     }
//                     catch (HttpRequestException e)
//                     {
//                         if (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
//                         {
//                             await Task.Delay(1000);
//                             Log.LogError($"Error 429 when trying to notify a student. Error {e.ToPrettyJson()}");
//                         }
//                         else
//                         {
//                             Log.LogError($"HttpError: {e.ToPrettyJson()}");
//                             break;
//                         }
//                     }
//                     catch (Exception e)
//                     {
//                         Log.LogError($"Error: {e.ToPrettyJson()}");
//                         break;
//                     }
//                 }
//
//                 // MessageSender.SendToUser(() => BotClient.SendTextMessageAsync(
//                 //     student.Id,
//                 //     message,
//                 //     replyMarkup: Keyboards.SendFeedbackKeyboard(subjectName)));
//             }
//         }
//
//         private Student[] GetStudentsToNotify(Course course, Group group, Subgroup subgroup, string subjectName, int studentsPercentToNotify)
//         {
//             if (studentsPercentToNotify == 0)
//                 return Array.Empty<Student>();
//
//             var students = Cache.Students.Get((course, group, subgroup));
//
//             // todo: обработать случай, когда у студента могут быть другие курсы помимо спецкурсов
//             if (course > Course.Second)
//             {
//                 students = students
//                     .Where(s => s.SpecialCourseNames.Contains(subjectName))
//                     .ToList();
//             }
//
//             var selectedStudentsCount = Math.Max(1, students.Count * studentsPercentToNotify / 100);
//
//             var random = new Random();
//
//             return students
//                 .OrderBy(_ => random.Next())
//                 .Take(selectedStudentsCount)
//                 .ToArray();
//         }
//     }
// }