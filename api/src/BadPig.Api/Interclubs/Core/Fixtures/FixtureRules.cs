using FluentValidation;

namespace BadPig.Interclubs.Core;

/// <summary>Validation rules of the fixtures, shared by the features that create or change them.</summary>
public static class FixtureRules
{
    public const int MaxOpponentLength = 100;
    public const int MaxPlaceLength = 200;

    public static IRuleBuilderOptions<T, DateOnly?> MustBeAFixtureDate<T>(this IRuleBuilder<T, DateOnly?> rule) =>
        rule.Must(date => date is not null).WithMessage("La date de la rencontre est obligatoire.");

    public static IRuleBuilderOptions<T, string?> MustBeAnOpponent<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxOpponentLength)
            .WithMessage($"Le club adverse est obligatoire ({MaxOpponentLength} caractères au plus).");

    public static IRuleBuilderOptions<T, string?> MustBeAVenue<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(venue => venue is not null && FixtureVenues.All.Contains(venue))
            .WithMessage("Indiquez si la rencontre se joue à domicile (home) ou à l'extérieur (away).");

    public static IRuleBuilderOptions<T, string?> MustBeAPlace<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(place => place is null || place.Trim().Length <= MaxPlaceLength)
            .WithMessage($"Le lieu est trop long ({MaxPlaceLength} caractères au plus).");
}
