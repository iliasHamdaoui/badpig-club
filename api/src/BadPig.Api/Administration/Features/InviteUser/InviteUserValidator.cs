using BadPig.Administration.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.InviteUser;

internal sealed class InviteUserValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom est obligatoire.")
            .Must(name => name is null || name.Trim().Length <= 200).WithMessage("Le nom est trop long (200 caractères au plus).");
        RuleFor(x => x.Email).NotEmpty().WithMessage("L'adresse e-mail est obligatoire.")
            .MustBeAnEmail(150);
        RuleFor(x => x.Group).Must(group => string.IsNullOrEmpty(group) || group is "members" or "board" or "admin")
            .WithMessage("Le groupe est « members », « board » ou « admin ».");
        RuleFor(x => x.MembershipKind).Must(kind => kind is null || MembershipKinds.IsValid(kind))
            .WithMessage("Le type d'adhésion est « internal » (licencié du club) ou « external » (licencié dans un autre club).");
    }
}
