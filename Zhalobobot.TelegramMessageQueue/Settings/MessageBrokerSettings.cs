namespace Zhalobobot.TelegramMessageQueue.Settings;

public static class MessageBrokerSettings
{
    internal static int UserMessagesLimit { get; set; } = 25; // note (d.stukov): max allowed ~30
    internal static int GroupMessagesPerMinuteLimit { get; set; } = 16; // note (d.stukov): max allowed ~20
}