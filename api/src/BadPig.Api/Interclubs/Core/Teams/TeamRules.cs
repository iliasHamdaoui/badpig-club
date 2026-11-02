using BadPig.Administration.Core;
using FluentValidation;

namespace BadPig.Interclubs.Core;

/// <summary>Validation rules of the teams, shared by the features that create or change them.</summary>
public static class TeamRules
{
    public const int MaxNameLength = 30;

    public static IRuleBuilderOptions<T, string?> MustBeATeamName<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(name => !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxNameLength)
            .WithMessage($"Le nom de l'équipe est obligatoire ({MaxNameLength} caractères au plus).");

    /// <summary>
    /// The club fields its own licensees: someone whose membership is "external" is licensed in another club and
    /// plays the interclubs for that one. A person without a validated membership is not refused here.
    /// </summary>
    public static bool CanPlayInterclubs(MembershipStatus? membership) => membership?.Kind != MembershipKinds.External;
}
