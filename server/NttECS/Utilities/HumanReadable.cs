namespace server.Utilities;

public static class HumanReadable
{
    /// <summary>
    /// Formats a number to a human readable string.
    /// Tera, Giga, Mega, Kilo, Thousand, Hundred
    /// </summary>
    public static string ToTGMK(long number)
    {
        var strLegend = number.ToString();
        if (number > 1000000000000)
            strLegend = $"{number / 1000000000000}T";
        else if (number > 1000000000)
            strLegend = $"{number / 1000000000}G";
        else if (number > 1000000)
            strLegend = $"{number / 1000000}M";
        else if (number > 1000)
            strLegend = $"{number / 1000}K";
        return strLegend;
    }
}