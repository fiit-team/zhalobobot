using Zhalobobot.Common.Models.Schedule;

namespace Zhalobobot.Bot.Cache;

public class DaysWithoutPairsCache : EntityCacheBase<DayWithoutPairs>
{
    public DaysWithoutPairsCache(DayWithoutPairs[] entities) : base(entities)
    {
    }
}