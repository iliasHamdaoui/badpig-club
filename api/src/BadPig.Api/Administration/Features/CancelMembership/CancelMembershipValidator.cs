using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.CancelMembership;

internal sealed class CancelMembershipValidator : AbstractValidator<CancelMembershipRequest>
{
    public CancelMembershipValidator() => RuleFor(x => x.Id).MustBeGuid();
}
