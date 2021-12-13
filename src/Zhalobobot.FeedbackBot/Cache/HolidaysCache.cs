using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Cache
{
    public class HolidaysCache : EntityCacheBase<DayAndMonth>
    {
        public HolidaysCache(DayAndMonth[] holidays) 
            : base(holidays)
        {
        }
    }
}