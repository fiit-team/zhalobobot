namespace Zhalobobot.TelegramMessageQueue.Settings;

public static class MessageBrokerSettings
{
    internal static int AllUserMessagesPerSecondLimit { get; set; } = 20; // note (d.stukov): max allowed ~30
    internal static int GroupMessagesPerMinuteLimit { get; set; } = 14; // note (d.stukov): max allowed ~20
}