using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Zhalobobot.Bot.Cache;

namespace Zhalobobot.Bot.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateCacheJob : IJob
    {
        private readonly EntitiesCache cache;
        
        public UpdateCacheJob(IServiceProvider container)
        {
            cache = container.GetRequiredService<EntitiesCache>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await cache.UpdateAll();
        }
    }
}