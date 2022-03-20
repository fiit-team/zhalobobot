using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Serialization;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.TelegramMessageQueue;

namespace Zhalobobot.Bot.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class NotifyStudentsJob : IJob
    {
        private MessageSender MessageSender { get; }
        private ITelegramBotClient BotClient { get; }
        private EntitiesCache Cache { get; }
        private ILogger<NotifyStudentsJob> Log { get; }

        public NotifyStudentsJob(MessageSender messageSender, ITelegramBotClient botClient, EntitiesCache cache, ILogger<NotifyStudentsJob> log)
        {
            MessageSender = messageSender;
            BotClient = botClient;
            Cache = cache;
            Log = log;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var courses = Cache.ActualSchedule(skipEndTimeCheck: false).ToArray();

            Log.LogInformation($"Select {courses.Length} courses to notify");

            foreach (var course in courses)
            {
                SendNotifyMessageToStudents(
                    course.Subject.Course,
                    course.Group,
                    course.Subgroup,
                    course.Subject.Name,
                    course.Subject.StudentsToNotifyPercent);
            }

            return Task.CompletedTask;
        }

        private void SendNotifyMessageToStudents(Course course, Group group, Subgroup subgroup, string subjectName, int studentsPercentToNotify)
        {
            Log.LogInformation($"Course: {course.ToPrettyJson()}, Group: {group.ToPrettyJson()}, Subgroup: {subgroup.ToPrettyJson()}");

            var studentsToNotify = GetStudentsToNotify(course, group, subgroup, studentsPercentToNotify);

            Log.LogInformation($"Selected students: {studentsToNotify.Select(s => s.Username).ToPrettyJson()}");

            foreach (var student in studentsToNotify)
            {
                var message = string.Join("\n",
                    $"Привет! Я догадываюсь, что у тебя закончилась пара по предмету {subjectName}.",
                    "Пожалуйста, оставь обратную связь по нему :)");

                MessageSender.SendToUser(() => BotClient.SendTextMessageAsync(
                    student.Id,
                    message,
                    replyMarkup: Keyboards.SendFeedbackKeyboard(subjectName)));
            }
        }

        private Student[] GetStudentsToNotify(Course course, Group group, Subgroup subgroup, int studentsPercentToNotify)
        {
            if (studentsPercentToNotify == 0)
                return Array.Empty<Student>();

            var students = Cache.Students.Get((course, group, subgroup));

            var selectedStudentsCount = Math.Max(1, students.Count * studentsPercentToNotify / 100);

            var random = new Random();

            return students
                .OrderBy(_ => random.Next())
                .Take(selectedStudentsCount)
                .ToArray();
        }
    }
}