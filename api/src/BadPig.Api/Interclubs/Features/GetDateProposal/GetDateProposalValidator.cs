using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.GetDateProposal;

internal sealed class GetDateProposalValidator : AbstractValidator<GetDateProposalRequest>
{
    public GetDateProposalValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
