namespace Zhalobobot.Common.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string Slice(this string text, int maxLength = 40)
        {
            return text.Length <= maxLength ? text : text[..(maxLength-2)] + "..";
        }

        public static (string First, string Second) SplitPair(this string text, char separator = '&')
        {
            var tokens = text.Split(new[] { separator }, 2);
            return (tokens[0], tokens[1]);
        }  
    }
}