using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.DeleteTeam;

internal sealed class DeleteTeamValidator : AbstractValidator<DeleteTeamRequest>
{
    public DeleteTeamValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
