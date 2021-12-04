using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Schedule;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Cache
{
    public class EntitiesCache
    {
        private readonly EntityCacheContainer<ScheduleItem, ScheduleItemCache> schedule;
        private readonly EntityCacheContainer<Student, StudentCache> student;

        private readonly IEntityCacheContainer[] allContainers;

        public EntitiesCache(IZhalobobotApiClient client)
        {
            schedule = new EntityCacheContainer<ScheduleItem, ScheduleItemCache>(client.Schedule.GetAll().GetResult, items => new ScheduleItemCache(items));
            student = new EntityCacheContainer<Student, StudentCache>(client.Student.GetAll().GetResult, items => new StudentCache(items));
            
            allContainers = new IEntityCacheContainer[]
            {
                schedule,
                student
            };
        }

        public ScheduleItemCache ScheduleItems => schedule.Cache;
        public StudentCache Students => student.Cache;
        
        public async Task UpdateAll()
        {
            foreach (var container in allContainers)
                await container.Update(true);
        }
    }
}