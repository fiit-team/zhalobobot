namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string Slice(this string text, int maxLength = 40)
        {
            return text.Length <= maxLength ? text : text[..(maxLength-2)] + "..";
        }
    }
}