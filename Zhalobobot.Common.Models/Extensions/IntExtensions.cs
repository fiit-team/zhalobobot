namespace Zhalobobot.Common.Models.Extensions
{
    public static class IntExtensions
    {
        public static string WithLeadingZeroIfLessThanTen(this int value)
            => value < 10 ? $"0{value}" : value.ToString();
    }
}