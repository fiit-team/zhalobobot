using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record Student(
        string Id, //telegram id
        string Username, // @...
        int Course, // аналогично год легко определить
        int Group, // при сообщении пользователя проверим, была ли запись
        int Subgroup, // о нём, если нет, предложим выбрать группу и подгруппу, чтобы предметы получить
        Name? Name);
}