using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record AbTestStudent(
        string TelegramId, 
        Name Name, 
        int? AdmissionYear, 
        int? GroupNumber, 
        int? SubgroupNumber, 
        bool InGroupA) 
        : Student(TelegramId, Name, AdmissionYear, GroupNumber, SubgroupNumber);
}