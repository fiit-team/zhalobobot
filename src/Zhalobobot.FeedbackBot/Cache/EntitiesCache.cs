using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Clients.Core.Extensions;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Reply;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Cache
{
    public class EntitiesCache
    {
        private readonly EntityCacheContainer<Common.Models.Schedule.Schedule, ScheduleItemCache> schedule;
        private readonly EntityCacheContainer<Student, StudentCache> students;
        private readonly EntityCacheContainer<StudentData, StudentDataCache> studentsData;
        private readonly EntityCacheContainer<Subject, SubjectCache> subjects;
        private readonly EntityCacheContainer<Reply, RepliesCache> replies;
        private readonly EntityCacheContainer<DayWithoutPairs, DaysWithoutPairsCache> daysWithoutPairs;

        private readonly IEntityCacheContainer[] allContainersToUpdate;

        public EntitiesCache(IZhalobobotApiClient client)
        {
            schedule = new EntityCacheContainer<Common.Models.Schedule.Schedule, ScheduleItemCache>(() => client.Schedule.GetAll().GetResult(), s => s.ShouldBeUpdated ? new ScheduleItemCache(s.Items) : ScheduleItems);
            students = new EntityCacheContainer<Student, StudentCache>(() => client.Student.GetAll().GetResult(), items => new StudentCache(items));
            subjects = new EntityCacheContainer<Subject, SubjectCache>(() => client.Subject.GetAll().GetResult(), items => new SubjectCache(items));
            studentsData = new EntityCacheContainer<StudentData, StudentDataCache>(() => client.Student.GetAllData().GetResult(), items => new StudentDataCache(items));
            daysWithoutPairs = new EntityCacheContainer<DayWithoutPairs, DaysWithoutPairsCache>(() => client.Schedule.GetDaysWithoutPairs().GetResult(), items => new DaysWithoutPairsCache(items));

            replies = new EntityCacheContainer<Reply, RepliesCache>(() => client.Reply.GetAll().GetResult(), items => new RepliesCache(items));
            replies.Update(true).Wait();

            allContainersToUpdate = new IEntityCacheContainer[]
            {
                schedule,
                students,
                subjects,
                studentsData,
                daysWithoutPairs
            };
        }

        public IEnumerable<ScheduleItem> ActualWeekSchedule(DateTime mondayDate) =>
            ScheduleItems.All.ActualWeekSchedule(mondayDate, DaysWithoutPairs.All);

        public IEnumerable<ScheduleItem> ActualWeekSchedule(Student student, WeekParity weekParity, DateTime mondayDate) =>
            ActualWeekSchedule(mondayDate).For(student, weekParity);
        
        public IEnumerable<ScheduleItem> ActualSchedule(bool skipEndTimeCheck) =>
            ScheduleItems.All.ActualDaySchedule(DateHelper.EkbTime, DaysWithoutPairs.All, skipEndTimeCheck);

        public IEnumerable<ScheduleItem> ActualSchedule(Student student, WeekParity weekParity, bool skipEndTimeCheck) =>
            ActualSchedule(skipEndTimeCheck).For(student, weekParity);

        private ScheduleItemCache ScheduleItems => schedule.Cache;
        public StudentCache Students => students.Cache;
        public SubjectCache Subjects => subjects.Cache;
        public StudentDataCache StudentData => studentsData.Cache;
        private DaysWithoutPairsCache DaysWithoutPairs => daysWithoutPairs.Cache;
        
        public RepliesCache Replies => replies.Cache;

        public async Task UpdateAll()
        {
            foreach (var container in allContainersToUpdate)
                await container.Update(true);
        }
    }
}