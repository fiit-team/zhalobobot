namespace Zhalobobot.TelegramMessageQueue.Settings;

public static class MessageSenderSettings
{
    internal static int SenderExecutionCallsPerSecondCount { get; set; } = 4;
    
    // note (d.stukov): max allowed ~30 messages per second
    internal static int AllUserMessagesPerSecondLimit { get; set; } = 20 / SenderExecutionCallsPerSecondCount;
    
    // note (d.stukov): max allowed ~20 messages per minute
    internal static int GroupMessagesPerMinuteLimit { get; set; } = 16 / SenderExecutionCallsPerSecondCount;
}