namespace Zhalobobot.Bot.Helpers;

public static class BotMessageHelper
{
    public const string ChatIsNotSupported = "Ваш чат не поддерживается администрацией ФИИТ.";
    public const string SentMessage = "Сообщение отправлено";
    public const string StartReplyDialog = "Начать общение";

    public const string StopReplyDialogStartPart = "Со студентом общается ";
    public static string StopReplyDialog(string username) => $"{StopReplyDialogStartPart}{username}";

    public static string GetUserFromReplyDialog(string message) => message.Replace(StopReplyDialogStartPart, "");

    public static bool IsReplyDialogNorForUser(string message, string username) =>
        message is StartReplyDialog or "Занять" // backward compability
        || (message.StartsWith("Занял") && !message.EndsWith(username))
        || (message.StartsWith(StopReplyDialogStartPart) && !message.EndsWith(username));
    
    public static string MessageReply(string reply) => $"На твоё сообщение ответили:\n\n{reply}\n\nЕсли хочешь продолжить общение, можешь ответить реплаем на это сообщение.";
}