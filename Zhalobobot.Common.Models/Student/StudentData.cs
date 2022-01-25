using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record StudentData(
        Course Course,
        Group Group,
        Subgroup Subgroup,
        string TelegramId,
        Name Name);
}