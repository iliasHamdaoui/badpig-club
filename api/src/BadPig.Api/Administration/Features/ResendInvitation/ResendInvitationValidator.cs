using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.ResendInvitation;

internal sealed class ResendInvitationValidator : AbstractValidator<ResendInvitationRequest>
{
    public ResendInvitationValidator() => RuleFor(x => x.Id).MustBeGuid();
}
