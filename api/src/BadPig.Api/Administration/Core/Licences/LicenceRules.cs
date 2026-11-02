using System.Text.RegularExpressions;
using FluentValidation;

namespace BadPig.Administration.Core;

/// <summary>What a licence number looks like, for the features that record one (the person's own, or the board's).</summary>
public static partial class LicenceRules
{
    public const int MaxLength = 20;

    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9-]*$")]
    private static partial Regex Allowed();

    /// <summary>What is stored: trimmed, or null when the person is clearing their licence.</summary>
    public static string? Clean(string? number) => string.IsNullOrWhiteSpace(number) ? null : number.Trim();

    /// <summary>True when the number can be stored: empty (no licence yet) or letters, digits and dashes.</summary>
    public static bool IsALicenceNumber(string? number)
    {
        var cleaned = Clean(number);
        return cleaned is null || (cleaned.Length <= MaxLength && Allowed().IsMatch(cleaned));
    }

    /// <summary>The licence is optional: an empty value removes the one on record.</summary>
    public static IRuleBuilderOptions<T, string?> MustBeALicenceNumber<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(number => IsALicenceNumber(number))
            .WithMessage($"Le numéro de licence ne contient que des lettres, des chiffres et des tirets ({MaxLength} caractères au plus).");
}
