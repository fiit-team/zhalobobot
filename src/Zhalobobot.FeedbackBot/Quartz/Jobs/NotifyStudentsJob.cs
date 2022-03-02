using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Serialization;

namespace Zhalobobot.Bot.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class NotifyStudentsJob : IJob
    {
        private ITelegramBotClient BotClient { get; }
        private EntitiesCache Cache { get; }
        private ILogger<NotifyStudentsJob> Log { get; }

        public NotifyStudentsJob(ITelegramBotClient botClient, EntitiesCache cache, ILogger<NotifyStudentsJob> log)
        {
            BotClient = botClient;
            Cache = cache;
            Log = log;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var ekbTime = DateHelper.EkbTime;
            var courses = Cache.ScheduleItems
                .GetByDayOfWeekAndEndsAtTime(ekbTime.DayOfWeek, ekbTime.ToTimeOnly());
            
            foreach (var course in courses)
            {
                if (course.Subgroup.HasValue)
                {
                    await SendNotifyMessageToStudents(course.Subject.Course, course.Group, course.Subgroup.Value, course.Subject.Name, course.Subject.StudentsToNotifyPercent);
                }
                else
                {
                    await SendNotifyMessageToStudents(course.Subject.Course, course.Group, Subgroup.First, course.Subject.Name, course.Subject.StudentsToNotifyPercent);
                    await SendNotifyMessageToStudents(course.Subject.Course, course.Group, Subgroup.Second, course.Subject.Name, course.Subject.StudentsToNotifyPercent);
                }
            }
            
            async Task SendNotifyMessageToStudents(Course course, Group group, Subgroup subgroup, string subjectName, int studentsPercentToNotify)
            {
                Log.LogInformation($"Course: {course.ToPrettyJson()}, Group: {group.ToPrettyJson()}, Subgroup: {subgroup.ToPrettyJson()}");
                var students = Cache.Students.Get((course, group, subgroup));
                
                var selectedStudentsCount = studentsPercentToNotify == 0 
                    ? 0 
                    : Math.Max(1, students.Count * studentsPercentToNotify / 100);
                    
                var random = new Random();
                    
                var selectedStudents = students
                    .OrderBy(_ => random.Next())
                    .Take(selectedStudentsCount)
                    .ToArray();
                
                Log.LogInformation($"Selected students: {selectedStudents.Select(s => s.Username).ToPrettyJson()}");

                foreach (var student in selectedStudents)
                {
                    var message = string.Join("\n",
                        $"Привет! Я догадываюсь, что у тебя закончилась пара по предмету {name}.",
                        "Пожалуйста, оставь обратную связь по нему, нужные кнопочки ниже :)");

                    await BotClient.SendTextMessageAsync(
                        student.Id, 
                        message, 
                        replyMarkup: Keyboards.SendFeedbackKeyboard(name));
                }
            }
        }
    }
}