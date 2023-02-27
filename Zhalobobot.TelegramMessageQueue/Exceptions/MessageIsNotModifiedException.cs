using Zhalobobot.TelegramMessageQueue.Exceptions;

namespace Zhalobobot.Bot.Models.Exceptions;

public class MessageIsNotModifiedException : BadRequestException
{
    /// <summary>
    /// Initializes a new object of the <see cref="MessageIsNotModifiedException"/> class
    /// </summary>
    /// <param name="message">The error message of this exception.</param>
    public MessageIsNotModifiedException(string message)
        : base(message)
    {
    }
}