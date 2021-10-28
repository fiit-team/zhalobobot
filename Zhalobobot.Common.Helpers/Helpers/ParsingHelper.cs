using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Helpers.Helpers
{
    public static class ParsingHelper
    {
        public static Name ParseName(object lastName, object firstName, object middleName)
            => new(lastName as string ?? "", firstName as string ?? "", middleName as string);

        public static int? ParseNullableInt(object value)
            => int.TryParse(value as string, out var result) ? result : null;
    }
}