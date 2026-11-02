using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.DisableAccount;

internal sealed class DisableAccountValidator : AbstractValidator<DisableAccountRequest>
{
    public DisableAccountValidator() => RuleFor(x => x.Id).MustBeGuid();
}
