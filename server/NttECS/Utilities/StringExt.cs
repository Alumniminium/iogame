using System;

namespace server.Utilities;

public static class StringExt
{
    public static string CenterString(string txt, int length)
    {
        var delta = MathF.Abs(txt.Length - length);
        for (var i = 0; i < delta; i++)
            if (i % 2 == 0)
                txt = " " + txt;
            else
                txt += " ";
        return txt;
    }
}