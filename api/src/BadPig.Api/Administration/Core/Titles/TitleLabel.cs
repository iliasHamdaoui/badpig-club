using FluentValidation;

namespace BadPig.Administration.Core;

public static class TitleLabel
{
    public const int MaxLength = 60;

    /// <summary>The label without the spaces around it (the validators have checked that it is 1 to 60 characters).</summary>
    public static string Normalize(string label) => label.Trim();

    /// <summary>FluentValidation rule: the label has 1 to 60 characters once trimmed.</summary>
    public static IRuleBuilderOptions<T, string?> MustBeATitleLabel<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(label => !string.IsNullOrWhiteSpace(label) && label.Trim().Length <= MaxLength)
            .WithMessage($"Nom de titre invalide (1 à {MaxLength} caractères).");
}
