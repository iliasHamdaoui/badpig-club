using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.CreateTeam;

internal sealed class CreateTeamValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name).MustBeATeamName();
        RuleFor(x => x.SeasonId).MustBeGuid();
    }
}
