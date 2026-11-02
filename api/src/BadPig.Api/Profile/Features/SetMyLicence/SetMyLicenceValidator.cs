using BadPig.Administration.Core;
using FluentValidation;

namespace BadPig.Profile.Features.SetMyLicence;

internal sealed class SetMyLicenceValidator : AbstractValidator<SetMyLicenceRequest>
{
    public SetMyLicenceValidator() => RuleFor(x => x.Number).MustBeALicenceNumber();
}
