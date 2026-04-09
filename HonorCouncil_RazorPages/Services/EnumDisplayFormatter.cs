using System.Text.RegularExpressions;

namespace HonorCouncil_RazorPages.Services;

public static partial class EnumDisplayFormatter
{
    public static string ToDisplayString<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        return EnumWordBoundaryRegex().Replace(value.ToString(), " $1").Trim();
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex EnumWordBoundaryRegex();
}
