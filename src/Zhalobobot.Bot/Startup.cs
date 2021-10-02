using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using Telegram.Bot;
using Zhalobobot.Bot.Repositories;
using Zhalobobot.Bot.Services;

namespace Zhalobobot.Bot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private BotConfiguration BotConfig { get; }
        private Settings Settings { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.BotConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
            this.Settings = configuration.GetSection("Settings").Get<Settings>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<ConfigureWebhook>();

            services.AddHttpClient("tgwebhook")
                    .AddTypedClient<ITelegramBotClient>(httpClient
                        => new TelegramBotClient(this.BotConfig.BotToken, httpClient));

            services.AddSingleton(this.Settings);
            services.AddSingleton<ISubjectsService, SubjectsService>();
            services.AddSingleton<IConversationService, ConversationService>();
            services.AddScoped<HandleUpdateService>();

            this.ConfigureReposiories(services);

            services.AddControllers()
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
                var token = this.BotConfig.BotToken;
                endpoints.MapControllerRoute(
                    name: "tgwebhook",
                    pattern: $"bot/{token}",
                    new { controller = "Webhook", action = "Post" });
                endpoints.MapControllers();
            });
        }

        private void ConfigureReposiories(IServiceCollection services)
        {
            var scopes = new[] { SheetsService.Scope.Spreadsheets };

            GoogleCredential credential;

            using (var stream = new FileStream(this.Settings.CredentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(scopes);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.Settings.ApplicationName,
            });

            services.AddSingleton(service.Spreadsheets);
            services.AddSingleton<IFeedbackRepository, GoogleSheetsRepository>();
        }
    }
}
