using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Helpers
{
    public static class SemesterHelper
    {
        public static Semester Current => DateHelper.EkbTime.Month is 1 or > 7 ? Semester.First : Semester.Second;
        // public static Semester Current => Semester.Second;
    }
}