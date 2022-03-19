namespace Zhalobobot.TelegramMessageQueue.Settings;

public static class MessageBrokerSettings
{
    internal static int BrokerExecutionCallsPerSecondCount { get; set; } = 4;
    
    // note (d.stukov): max allowed ~30 messages per second
    internal static int AllUserMessagesPerSecondLimit { get; set; } = 20 / BrokerExecutionCallsPerSecondCount;
    
    // note (d.stukov): max allowed ~20 messages per minute
    internal static int GroupMessagesPerMinuteLimit { get; set; } = 16 / BrokerExecutionCallsPerSecondCount;
}