namespace Zhalobobot.Bot.Cache
{
    public abstract class EntityCacheBase<TEntity>
        where TEntity : class
    {
        protected EntityCacheBase(TEntity[] entities)
            => All = entities;

        public TEntity[] All { get; }
    }
}