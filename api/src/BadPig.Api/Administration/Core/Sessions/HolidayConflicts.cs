using FluentValidation;

namespace BadPig.Administration.Core;

/// <summary>Checks a period of school holidays against the season and the other periods.</summary>
public static class HolidayConflicts
{
    public const int NameMaxLength = 60;

    /// <summary>A message for the board when the period falls outside the season; null when all is well.</summary>
    public static string? OutsideSeason(Season season, DateOnly startsOn, DateOnly endsOn) =>
        startsOn < season.StartsOn || endsOn > season.EndsOn
            ? $"Ces dates sortent de la saison « {season.Name} » ({season.StartsOn:dd/MM/yyyy} – {season.EndsOn:dd/MM/yyyy})."
            : null;

    /// <summary>A message for the board when the period overlaps another one of the season; null when all is well.</summary>
    public static string? Overlap(IEnumerable<SchoolHoliday> others, DateOnly startsOn, DateOnly endsOn) =>
        others.FirstOrDefault(h => h.StartsOn <= endsOn && startsOn <= h.EndsOn) is { } other
            ? $"Ces dates chevauchent « {other.Name} »."
            : null;
}

/// <summary>FluentValidation rules shared by the features that create or change a period of school holidays.</summary>
public static class HolidayValidationRules
{
    /// <summary>The name is not blank and at most 60 characters.</summary>
    public static IRuleBuilderOptions<T, string?> MustBeAHolidayName<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= HolidayConflicts.NameMaxLength)
            .WithMessage($"Le nom des vacances est obligatoire ({HolidayConflicts.NameMaxLength} caractères au plus).");
}
