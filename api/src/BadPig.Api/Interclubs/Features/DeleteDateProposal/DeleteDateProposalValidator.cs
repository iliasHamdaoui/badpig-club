using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.DeleteDateProposal;

internal sealed class DeleteDateProposalValidator : AbstractValidator<DeleteDateProposalRequest>
{
    public DeleteDateProposalValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
