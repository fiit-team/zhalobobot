using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Bot.Settings
{
    public class StudentSettings
    {
        public Course Course { get; init; }

        public Group? Group { get; init; } = null;

        public Subgroup? Subgroup { get; init; } = null;
    }
}
