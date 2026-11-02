using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.UpdateTeam;

internal sealed class UpdateTeamValidator : AbstractValidator<UpdateTeamRequest>
{
    public UpdateTeamValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Name).MustBeATeamName();
        RuleFor(x => x.Body.SeasonId).MustBeGuid();
    }
}
