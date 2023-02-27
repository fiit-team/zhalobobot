using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Zhalobobot.Bot.Services
{
    // public class ConfigureWebhook : IHostedService
    // {
    //     private ILogger<ConfigureWebhook> Logger { get; }
    //     private IServiceProvider Services { get; }
    //     private BotConfiguration BotConfig { get; }
    //
    //     public ConfigureWebhook(
    //         ILogger<ConfigureWebhook> logger,
    //         IServiceProvider serviceProvider,
    //         IConfiguration configuration)
    //     {
    //         Logger = logger;
    //         Services = serviceProvider;
    //         BotConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
    //     }
    //
    //     public async Task StartAsync(CancellationToken cancellationToken)
    //     {
    //         using var scope = Services.CreateScope();
    //         var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
    //
    //         var webhookAddress = @$"{BotConfig.HostAddress}/bot/{BotConfig.TelegramBotToken}";
    //         Logger.LogInformation("Setting webhook: {}", webhookAddress);
    //         await botClient.SetWebhookAsync(
    //             webhookAddress,
    //             allowedUpdates: Array.Empty<UpdateType>(),
    //             cancellationToken: cancellationToken);
    //     }
    //
    //     public Task StopAsync(CancellationToken cancellationToken)
    //     {
    //         return Task.CompletedTask;
    //     }
    // }
}
