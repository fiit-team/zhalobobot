namespace Zhalobobot.Bot.Cache
{
    public class HolidaysCache : EntityCacheBase<DateOnlyRecord>
    {
        public HolidaysCache(DateOnlyRecord[] holidays) 
            : base(holidays)
        {
        }
    }
}