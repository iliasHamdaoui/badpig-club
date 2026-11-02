using FluentValidation;

namespace BadPig.Administration.Core;

/// <summary>FluentValidation rules shared by the features that create or change a season.</summary>
public static class SeasonValidationRules
{
    public const int NameMaxLength = 60;
    public const decimal MaxFee = 9999.99m;

    /// <summary>A fee (when given: the "required" rule is separate) is between 0 and 9999.99, with at most two decimals.</summary>
    public static IRuleBuilderOptions<T, decimal?> MustBeAFee<T>(this IRuleBuilder<T, decimal?> rule) =>
        rule.Must(fee => fee is null || (fee >= 0 && fee <= MaxFee && decimal.Round(fee.Value, 2) == fee))
            .WithMessage($"La cotisation doit être comprise entre 0 et {MaxFee} €, avec deux décimales au plus.");

    /// <summary>The number of days (when given) of the grace period (the previous season's membership still accepted).</summary>
    public static IRuleBuilderOptions<T, int?> MustBeAGracePeriod<T>(this IRuleBuilder<T, int?> rule) =>
        rule.Must(days => days is null || days is >= 0 and <= SeasonRules.MaxGraceDays)
            .WithMessage($"La tolérance est comprise entre 0 et {SeasonRules.MaxGraceDays} jours.");

    /// <summary>The name, when given, is not blank and at most 60 characters.</summary>
    public static IRuleBuilderOptions<T, string?> MustBeASeasonName<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(name => name is null || (name.Trim().Length > 0 && name.Trim().Length <= NameMaxLength))
            .WithMessage($"Le nom de la saison fait de 1 à {NameMaxLength} caractères.");
}
