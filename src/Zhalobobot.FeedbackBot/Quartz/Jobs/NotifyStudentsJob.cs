using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Telegram.Bot;
using Zhalobobot.Bot.Helpers;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Schedule.Requests;
using Zhalobobot.Common.Models.Serialization;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Bot.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class NotifyStudentsJob : IJob
    {
        private const int Percent = 20;
        private IZhalobobotApiClient Client { get; }
        private ITelegramBotClient BotClient { get; }
        private ILogger<NotifyStudentsJob> Log { get; }

        public NotifyStudentsJob(IZhalobobotApiClient client, ILogger<NotifyStudentsJob> log, ITelegramBotClient botClient)
        {
            Client = client;
            Log = log;
            BotClient = botClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Log.LogInformation($"TRIGGERED IN TIME:\nHour:{DateTime.Now.Hour}\nMinute:{DateTime.Now.Minute}\nDay:{DateTime.Now.DayOfWeek}");
            var getScheduleRequest = new GetScheduleByDayOfWeekHourAndMinuteRequest(DateTime.Now.DayOfWeek,
                new HourAndMinute(DateTime.Now.Hour, DateTime.Now.Minute));
            
            // var getScheduleRequest = new GetScheduleByDayOfWeekHourAndMinuteRequest(DayOfWeek.Monday,
            //     new HourAndMinute(10, 30));
            
            var courses = await Client.Schedule.GetByDayOfWeekAndEndsAtHourAndMinute(getScheduleRequest).GetResult();

            Log.LogInformation($"Courses: {courses.Select(c => c.Subject.Name).ToPrettyJson()}");

            foreach (var course in courses)
            {
                if (course.Subgroup.HasValue)
                {
                    await SendNotifyMessageToStudents(course.Subject.Course, course.Group, course.Subgroup.Value, course.Subject.Name);
                }
                else
                {
                    await SendNotifyMessageToStudents(course.Subject.Course, course.Group, Subgroup.First, course.Subject.Name);
                    await SendNotifyMessageToStudents(course.Subject.Course, course.Group, Subgroup.Second, course.Subject.Name);
                }
            }
            
            async Task SendNotifyMessageToStudents(Course course, Group group, Subgroup subgroup, string name)
            {
                Log.LogInformation($"Course: {course.ToPrettyJson()}, Group: {group.ToPrettyJson()}, Subgroup: {subgroup.ToPrettyJson()}");
                var request = new GetStudentsByCourseAndGroupAndSubgroupRequest(course, group, subgroup);
                var students = await Client.Student.Get(request).GetResult();
                
                var selectedStudentsCount = Math.Max(1, students.Length * Percent / 100);
                    
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
                        replyMarkup: WellKnownKeyboards.SendFeedbackKeyboard(name));
                }
            }
        }
    }
}