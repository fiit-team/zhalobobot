using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Zhalobobot.Bot.Services;

public class ConfigurePolling : BackgroundService
{
    private readonly ITelegramBotClient botClient;
    private readonly HandleUpdateService updateHandlers;
    private readonly ILogger<ConfigurePolling> logger;

    public ConfigurePolling(
        ITelegramBotClient botClient,
        IServiceScopeFactory factory,
        ILogger<ConfigurePolling> logger)
    {
        this.botClient = botClient;
        updateHandlers = factory.CreateScope().ServiceProvider.GetRequiredService<HandleUpdateService>();
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new ReceiverOptions
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message,
                UpdateType.PollAnswer,
                UpdateType.CallbackQuery,
                UpdateType.MyChatMember
            },
            
        };
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await botClient.ReceiveAsync(
                    updateHandler: updateHandlers.HandleUpdateAsync,
                    pollingErrorHandler: updateHandlers.PollingErrorHandler,
                    receiverOptions: options,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError("Polling failed with exception: {Exception}", ex);

                await Task.Delay(500, stoppingToken);
            }
        }
    }
}