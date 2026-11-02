using FluentValidation;

namespace BadPig.Shop.Core;

/// <summary>Validation rules of the products and variants, shared by the features that create or change them.</summary>
public static class ProductRules
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 1000;
    public const int MaxPriceCents = 100_000;
    public const int MaxQuantity = 100_000;
    public const int MaxNoteLength = 200;

    public static IRuleBuilderOptions<T, string?> MustBeAProductName<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxNameLength)
            .WithMessage($"Le nom est obligatoire ({MaxNameLength} caractères au plus).");

    public static IRuleBuilderOptions<T, string?> MustBeAProductDescription<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(text => text is null || text.Trim().Length <= MaxDescriptionLength)
            .WithMessage($"La description est trop longue ({MaxDescriptionLength} caractères au plus).");

    public static IRuleBuilderOptions<T, string?> MustBeAVariantLabel<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(label => !string.IsNullOrWhiteSpace(label) && label.Trim().Length <= MaxNameLength)
            .WithMessage($"Le libellé est obligatoire ({MaxNameLength} caractères au plus).");

    public static IRuleBuilderOptions<T, int?> MustBeAPrice<T>(this IRuleBuilder<T, int?> rule) =>
        rule.Must(price => price is >= 1 and <= MaxPriceCents)
            .WithMessage($"Le prix est compris entre 1 centime et {MaxPriceCents / 100} €.");

    /// <summary>A number of units, from <paramref name="min"/> to the maximum.</summary>
    public static IRuleBuilderOptions<T, int?> MustBeAQuantity<T>(this IRuleBuilder<T, int?> rule, int min) =>
        rule.Must(quantity => quantity >= min && quantity <= MaxQuantity)
            .WithMessage($"La quantité est comprise entre {min} et {MaxQuantity}.");

    public static IRuleBuilderOptions<T, int?> MustBeAThreshold<T>(this IRuleBuilder<T, int?> rule) =>
        rule.Must(threshold => threshold is >= 0 and <= MaxQuantity)
            .WithMessage($"Le seuil d'alerte est compris entre 0 et {MaxQuantity} (0 = pas d'alerte).");

    public static IRuleBuilderOptions<T, string?> MustBeAStockNote<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(note => note is null || note.Trim().Length <= MaxNoteLength)
            .WithMessage($"La note est trop longue ({MaxNoteLength} caractères au plus).");
}
