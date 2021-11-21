using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Common.Models.Schedule
{
    public record ScheduleItem(
        Subject.Subject Subject,
        EventTime EventTime,
        Group Group,
        Subgroup? Subgroup,
        string Cabinet,
        string Teacher);
}