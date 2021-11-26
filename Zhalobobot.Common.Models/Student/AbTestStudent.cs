using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record AbTestStudent(
        string TelegramId,
        Name? Name,
        int? Course,
        int? GroupNumber,
        int? SubgroupNumber,
        bool InGroupA)
        : OldStudent(TelegramId, Name, Course, GroupNumber, SubgroupNumber);
}