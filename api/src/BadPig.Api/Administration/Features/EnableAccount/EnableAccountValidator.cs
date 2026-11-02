using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.EnableAccount;

internal sealed class EnableAccountValidator : AbstractValidator<EnableAccountRequest>
{
    public EnableAccountValidator() => RuleFor(x => x.Id).MustBeGuid();
}
