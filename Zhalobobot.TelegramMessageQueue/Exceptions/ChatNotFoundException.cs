namespace Zhalobobot.TelegramMessageQueue.Exceptions;

public class ChatNotFoundException : BadRequestException
{
    /// <summary>
    /// Initializes a new object of the <see cref="ChatNotFoundException"/> class
    /// </summary>
    /// <param name="message">The error message of this exception.</param>
    public ChatNotFoundException(string message)
        : base(message)
    {
    }
}