using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;

namespace Zhalobobot.Bot.Helpers;

public class MessageWithEntitiesStringBuilder
{
    private readonly List<MessageEntity> internalEntities;
    private readonly StringBuilder builder;

    public MessageWithEntitiesStringBuilder()
    {
        internalEntities = new List<MessageEntity>();
        builder = new StringBuilder();
    }

    public (string Message, IEnumerable<MessageEntity> Entities) Build()
        => (builder.ToString(), internalEntities);

    public void AppendLine(string text = "") => builder.AppendLine(text);
    public void Append(string text = "") => builder.Append(text);

    public void AppendEntitiesLine(string text, MessageEntity[]? entities)
    {
        if (entities != null && entities.Any())
        {
            internalEntities.AddRange(AddOffsetToEntities(entities));
        }
        builder.AppendLine(text);
    }
    
    public void AppendEntities(string text, MessageEntity[]? entities)
    {
        if (entities != null && entities.Any())
        {
            internalEntities.AddRange(AddOffsetToEntities(entities));
        }
        builder.Append(text);
    }

    private IEnumerable<MessageEntity> AddOffsetToEntities(IEnumerable<MessageEntity> entities)
    {
        return entities.Select(entity => new MessageEntity
        {
            Offset = entity.Offset + builder.Length,
            Language = entity.Language,
            Length = entity.Length,
            Type = entity.Type,
            Url = entity.Url,
            User = entity.User
        });
    }
}