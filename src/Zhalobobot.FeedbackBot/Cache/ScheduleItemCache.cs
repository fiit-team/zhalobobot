using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Bot.Cache
{
    public class ScheduleItemCache : EntityCacheBase<ScheduleItem>
    {
        public ScheduleItemCache(ScheduleItem[] scheduleItems)
            : base(scheduleItems)
        {
        }
    }
}