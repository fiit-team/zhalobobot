using System.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Telegram.Bot;
using Vostok.Applications.AspNetCore;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Api.Repositories.Feedback;
using Zhalobobot.Bot.Api.Repositories.FeedbackChat;
using Zhalobobot.Bot.Api.Repositories.FiitStudentsData;
using Zhalobobot.Bot.Api.Repositories.Replies;
// using Zhalobobot.Bot.Api.Repositories.Schedule;
using Zhalobobot.Bot.Api.Repositories.Students;
using Zhalobobot.Bot.Api.Repositories.Subjects;
using Zhalobobot.Bot.Api.Services;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Quartz.Extensions;
using Zhalobobot.Bot.Quartz.Jobs;
using Zhalobobot.Bot.Services;
using Zhalobobot.Bot.Services.Handlers;
using Zhalobobot.Bot.Settings;
using Zhalobobot.TelegramMessageQueue;
// using Zhalobobot.TelegramMessageQueue.Settings;

namespace Zhalobobot.Bot;

[RequiresSecretConfiguration(typeof(BotSecrets))]
public class BotApplication : VostokNetCoreApplication
{
    public override void Setup(IVostokNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
    {
        builder.SetupGenericHost(b =>
        {
            b.ConfigureServices((_, services) => RegisterServices(services, environment));
        });
    }
    
    public void RegisterServices(IServiceCollection services, IVostokHostingEnvironment environment)
    {
        services.AddHostedService<ConfigurePolling>();

        services.AddHttpClient("tgpolling")
                .AddTypedClient<ITelegramBotClient>(httpClient
                    => new TelegramBotClient(environment.SecretConfigurationProvider.Get<BotSecrets>().BotConfiguration.TelegramBotToken, httpClient));

        ConfigureRepositories(services);
        RegisterQuartz(services);
        RegisterServices(services);
        RegisterCache(services);

        Settings.Settings Settings = environment.SecretConfigurationProvider.Get<BotSecrets>().Settings;

        services.AddSingleton(Settings);


        services.AddControllers(options => options.UseDateOnlyTimeOnlyStringConverters())
            .AddJsonOptions(options => options.UseDateOnlyTimeOnlyStringConverters())
            .AddNewtonsoftJson();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            // endpoints.MapControllerRoute(
            //     "tgwebhook",
            //     $"bot/{BotConfig.TelegramBotToken}",
            //     new { controller = "Webhook", action = "Post" });
            endpoints.MapControllers();
        });
    }

    private static void RegisterServices(IServiceCollection services)
    {            
        services.AddSingleton<IZhalobobotServices, ZhalobobotServices>();
        services.AddSingleton<IPollService, PollService>();
        services.AddSingleton<IConversationService, ConversationService>();
        // services.AddSingleton<IScheduleMessageService, ScheduleMessageService>();
        services.AddScoped<UpdateHandlerAdmin>();
        services.AddScoped<HandleUpdateService>();
        // todo: put MessageSenderSettings in MessageSender arguments
        // services.AddSingleton<MessageSenderSettings>();
        // services.AddSingleton<MessageSender>();
        
    }

    private static void RegisterQuartz(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            // q.AddJobAndTrigger<NotifyStudentsJob>("NotifyDuringStudyYearTrigger", configuration);
            
            q.AddJobAndTrigger<UpdateCacheJob>(SimpleScheduleBuilder.Create().WithIntervalInMinutes(1).RepeatForever());
            // q.AddJobAndTrigger<UpdateScheduleMessageJob>("0 * * * * ?");
        });
        
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }

    private static void RegisterCache(IServiceCollection services)
        => services.AddSingleton<EntitiesCache>();

    private static void ConfigureRepositories(IServiceCollection services)
    {
        services.AddSingleton<IFeedbackRepository, FeedbackRepository>();
        services.AddSingleton<IReplyRepository, ReplyRepository>();
        services.AddSingleton<ISubjectRepository, SubjectRepository>();
        // services.AddSingleton<IScheduleRepository, ScheduleRepository>();
        services.AddSingleton<IStudentRepository, StudentRepository>();
        services.AddSingleton<IFiitStudentsDataRepository, FiitStudentsDataRepository>();
        services.AddSingleton<IFeedbackChatRepository, FeedbackChatRepository>();
    }
}