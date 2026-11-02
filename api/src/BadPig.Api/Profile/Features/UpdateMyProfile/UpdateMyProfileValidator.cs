using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Profile.Features.UpdateMyProfile;

internal sealed class UpdateMyProfileValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Le nom est obligatoire.")
            .Must(name => name is null || name.Trim().Length <= 200).WithMessage("Le nom est trop long (200 caractères au plus).");
        RuleFor(x => x.Email).NotEmpty().WithMessage("L'adresse e-mail est obligatoire.")
            .MustBeAnEmail(254);
    }
}
