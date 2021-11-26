namespace Zhalobobot.Common.Helpers
{
    public static class Utils
    {
        public static string Join(char separator, params object[] values)
        {
            return string.Join(separator, values);
        }
    }
}
