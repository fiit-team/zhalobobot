using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record Student(
        string TelegramId,
        Name? Name,
        int? AdmissionYear,
        int? GroupNumber,
        int? SubgroupNumber);
}