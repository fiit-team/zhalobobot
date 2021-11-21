using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Helpers
{
    public static class SemesterHelper
    {
        public static Semester Current => DateTime.Now.Month is 1 or > 7 ? Semester.First : Semester.Second;
    }
}