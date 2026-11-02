using System.Globalization;
using System.Text;

namespace BadPig.Shared.Core;

/// <summary>
/// Alphabetical order for names: case and accents are ignored ("Émilie" comes between "Dora" and "Fanny", not after "Zoé").
/// The API runs with invariant globalization, where culture comparers fall back to plain code-point order.
/// </summary>
public sealed class AlphabeticalComparer : IComparer<string?>
{
    public static readonly AlphabeticalComparer Instance = new();

    public int Compare(string? x, string? y) => string.Compare(Fold(x), Fold(y), StringComparison.OrdinalIgnoreCase);

    private static string Fold(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        var builder = new StringBuilder(text.Length);
        foreach (var c in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }
        return builder.ToString();
    }
}
