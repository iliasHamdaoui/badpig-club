using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.RemoveTeamMember;

internal sealed class RemoveTeamMemberValidator : AbstractValidator<RemoveTeamMemberRequest>
{
    public RemoveTeamMemberValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.AccountId).MustBeGuid();
    }
}
