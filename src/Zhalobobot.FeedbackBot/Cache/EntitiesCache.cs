using System.Collections.Generic;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Clients.Core.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Reply;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Cache
{
    public class EntitiesCache
    {
        private readonly EntityCacheContainer<ScheduleItem, ScheduleItemCache> schedule;
        private readonly EntityCacheContainer<Student, StudentCache> students;
        private readonly EntityCacheContainer<StudentData, StudentDataCache> studentsData;
        private readonly EntityCacheContainer<Subject, SubjectCache> subjects;
        private readonly EntityCacheContainer<DayAndMonth, HolidaysCache> holidays;
        private readonly EntityCacheContainer<Reply, RepliesCache> replies;

        private readonly IEntityCacheContainer[] allContainers;

        public EntitiesCache(IZhalobobotApiClient client)
        {
            schedule = new EntityCacheContainer<ScheduleItem, ScheduleItemCache>(() => client.Schedule.GetAll().GetResult(), items => new ScheduleItemCache(items));
            students = new EntityCacheContainer<Student, StudentCache>(() => client.Student.GetAll().GetResult(), items => new StudentCache(items));
            subjects = new EntityCacheContainer<Subject, SubjectCache>(() => client.Subject.GetAll().GetResult(), items => new SubjectCache(items));
            holidays = new EntityCacheContainer<DayAndMonth, HolidaysCache>(() => client.Schedule.GetHolidays().GetResult(), items => new HolidaysCache(items));
            studentsData = new EntityCacheContainer<StudentData, StudentDataCache>(() => client.Student.GetAllData().GetResult(), items => new StudentDataCache(items));
            
            replies = new EntityCacheContainer<Reply, RepliesCache>(async () => new RepliesCache(System.Array.Empty<Reply>()));
            replies.Update(true).Wait();

            allContainers = new IEntityCacheContainer[]
            {
                schedule,
                students,
                subjects,
                holidays,
                studentsData
            };
        }

        public ScheduleItemCache ScheduleItems => schedule.Cache;
        public StudentCache Students => students.Cache;
        public SubjectCache Subjects => subjects.Cache;
        public HolidaysCache Holidays => holidays.Cache;
        public StudentDataCache StudentData => studentsData.Cache;
        
        public RepliesCache Replies => replies.Cache;

        public async Task UpdateAll()
        {
            foreach (var container in allContainers)
                await container.Update(true);
        }
    }
}