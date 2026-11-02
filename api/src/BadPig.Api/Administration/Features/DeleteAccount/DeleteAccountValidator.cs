using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.DeleteAccount;

internal sealed class DeleteAccountValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountValidator() => RuleFor(x => x.Id).MustBeGuid();
}
