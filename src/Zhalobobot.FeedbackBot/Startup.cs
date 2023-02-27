using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Telegram.Bot;
using Zhalobobot.Bot.Api.Repositories.Feedback;
using Zhalobobot.Bot.Api.Repositories.FeedbackChat;
using Zhalobobot.Bot.Api.Repositories.FiitStudentsData;
using Zhalobobot.Bot.Api.Repositories.Replies;
using Zhalobobot.Bot.Api.Repositories.Schedule;
using Zhalobobot.Bot.Api.Repositories.Students;
using Zhalobobot.Bot.Api.Repositories.Subjects;
using Zhalobobot.Bot.Api.Services;
using Zhalobobot.Bot.Cache;
using Zhalobobot.Bot.Settings;
using Zhalobobot.Bot.Quartz.Extensions;
using Zhalobobot.Bot.Quartz.Jobs;
using Zhalobobot.Bot.Schedule;
using Zhalobobot.Bot.Services;
using Zhalobobot.Bot.Services.Handlers;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.TelegramMessageQueue;
using Zhalobobot.TelegramMessageQueue.Settings;

namespace Zhalobobot.Bot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private BotConfiguration BotConfig { get; }
        private Settings.Settings Settings { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            BotConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            Settings = configuration.GetSection("Settings").Get<Settings.Settings>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<ConfigurePolling>();

            services.AddHttpClient("tgpolling")
                    .AddTypedClient<ITelegramBotClient>(httpClient
                        => new TelegramBotClient(BotConfig.TelegramBotToken, httpClient));

            ConfigureRepositories(services);
            RegisterQuartz(services, Configuration);
            RegisterServices(services);
            RegisterCache(services);

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
            services.AddSingleton<IScheduleMessageService, ScheduleMessageService>();
            services.AddScoped<UpdateHandlerAdmin>();
            services.AddScoped<HandleUpdateService>();
            // todo: put MessageSenderSettings in MessageSender arguments
            services.AddSingleton<MessageSenderSettings>();
            services.AddSingleton<MessageSender>();
            
        }

        private static void RegisterQuartz(IServiceCollection services, IConfiguration configuration)
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
            services.AddSingleton<IScheduleRepository, ScheduleRepository>();
            services.AddSingleton<IStudentRepository, StudentRepository>();
            services.AddSingleton<IFiitStudentsDataRepository, FiitStudentsDataRepository>();
            services.AddSingleton<IFeedbackChatRepository, FeedbackChatRepository>();
        }
    }
}
