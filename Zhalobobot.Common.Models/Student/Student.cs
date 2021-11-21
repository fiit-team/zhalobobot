using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record Student(
        string Id, //telegram id
        string Username, // @...
        Course Course, // аналогично год легко определить
        Group Group, // при сообщении пользователя проверим, была ли запись
        Subgroup Subgroup, // о нём, если нет, предложим выбрать группу и подгруппу, чтобы предметы получить
        Name? Name);
}