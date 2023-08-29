using System.Globalization;
using System.Text.RegularExpressions;

namespace Domain.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str.Length > 1) return char.ToLowerInvariant(str[0]) + str.Substring(1);
        return str;
    }

    public static string ToTitleCase(this string str)
    {
        var ti = CultureInfo.CurrentCulture.TextInfo;
        return ti.ToTitleCase(str.ToLower());
    }
}