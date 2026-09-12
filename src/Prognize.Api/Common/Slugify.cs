using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Prognize.Api.Common;

public static partial class Slugify
{
    public static string From(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = NonAlphanumeric().Replace(sb.ToString().ToLowerInvariant(), "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "org" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
