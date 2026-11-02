using BadPig.Administration.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.SetMemberLicence;

internal sealed class SetMemberLicenceValidator : AbstractValidator<SetMemberLicenceRequest>
{
    public SetMemberLicenceValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Number).MustBeALicenceNumber();
    }
}
