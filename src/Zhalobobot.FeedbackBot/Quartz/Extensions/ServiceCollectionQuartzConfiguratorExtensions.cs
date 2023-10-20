using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Quartz;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Settings;

namespace Zhalobobot.Bot.Quartz.Extensions
{
    public static class ServiceCollectionQuartzConfiguratorExtensions
    {
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            string triggerName,
            string cronSchedule)
            where T : IJob
        {
            if (string.IsNullOrEmpty(cronSchedule))
                throw new ValidationException($"No Quartz.NET Cron schedule found in configuration at Quartz:{triggerName}");

            var jobKey = new JobKey(typeof(T).Name);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(triggerName)
                .WithCronSchedule(cronSchedule));
        }
        
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            SimpleScheduleBuilder scheduleBuilder)
            where T : IJob
        {
            var jobKey = new JobKey(typeof(T).Name);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(Guid.NewGuid().ToString())
                .WithSimpleSchedule(scheduleBuilder));
        }
        
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            string cronSchedule)
            where T : IJob
        {
            var jobKey = new JobKey(typeof(T).Name);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(Guid.NewGuid().ToString())
                .WithCronSchedule(cronSchedule));
        }
    }
}