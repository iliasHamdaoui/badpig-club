using BadPig.Administration.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.ValidateMembership;

internal sealed class ValidateMembershipValidator : AbstractValidator<ValidateMembershipRequest>
{
    public ValidateMembershipValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Kind).Must(kind => kind is null || MembershipKinds.IsValid(kind))
            .WithMessage("Le type d'adhésion est « internal » (licencié du club) ou « external » (licencié dans un autre club).");
    }
}
