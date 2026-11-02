using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.AddTeamMember;

internal sealed class AddTeamMemberValidator : AbstractValidator<AddTeamMemberRequest>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.AccountId).MustBeGuid();
    }
}
