using System;
using System.Threading.Tasks;
using Zhalobobot.Common.Models.Exceptions;

namespace Zhalobobot.Bot.Cache
{
    public class EntityCacheContainer<TEntity, TCache> : IEntityCacheContainer
        where TEntity : class
        where TCache : class
    {
        private readonly Func<Task<TCache>> construct;

        private volatile TCache? cache;

        public EntityCacheContainer(Func<Task<TCache>> construct)
        {
            this.construct = construct;
        }

        public EntityCacheContainer(
            Func<Task<TEntity>> fetch,
            Func<TEntity, TCache> construct)
            : this(async () => construct(await fetch()))
        {
        }

        public EntityCacheContainer(
            Func<Task<TEntity[]>> fetch, 
            Func<TEntity[], TCache> construct)
            : this(async () => construct(await fetch()))
        {
        }

        public TCache Cache 
            => cache ?? throw new CacheNotInitializedException($"Cache of type '{EntityType}' is not initialized yet.");

        public async Task Update(bool ensureSuccess)
        {
            await Task.Yield();    //NOTE(d.stukov, 04.12.21): to make task execution really async
            
            try
            {
                cache = await construct();
            }
            catch (Exception)
            {
                if (ensureSuccess)
                    throw;
            }
        }

        private static string EntityType
            => typeof(TEntity).Name;
    }
}