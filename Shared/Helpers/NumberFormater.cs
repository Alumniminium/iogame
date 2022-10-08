namespace server.Helpers
{
    public static class StringExtensions
    {
        public static string FormatKMB(this int number)
        {
            if (number < 1000)
                return number.ToString();
            if (number < 1000000)
                return $"{number / 1000}K";
            if (number < 1000000000)
                return $"{number / 1000000}M";
            return $"{number / 1000000000}B";
        }

        public static string ToLength(this string str, int length)
        {
            if (str.Length > length)
                return str[..length];
            return str.PadRight(length);
        }
    }
}