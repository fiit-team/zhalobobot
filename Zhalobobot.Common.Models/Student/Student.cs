using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record Student(
        string TelegramId,
        Name? Name,
        int? AdmissionYear,
        int? GroupNumber,
        int? SubgroupNumber)
    {
        public string? GetGroup()
        {
            string? group = null;
            if (GroupNumber != null)
                group = $"тр-{GroupNumber}";
            if (SubgroupNumber != null)
                group += $"0{SubgroupNumber}";

            return group;
        }
    }
}