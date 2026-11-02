using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ListDateProposals;

internal sealed class ListDateProposalsValidator : ListQueryValidator<ListDateProposalsRequest>
{
    public ListDateProposalsValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("createdAt", "team");
        RuleFor(x => x.TeamId).Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Identifiant invalide.");
        RuleFor(x => x.Status).Must(status => string.IsNullOrEmpty(status) || ProposalStatuses.All.Contains(status))
            .WithMessage($"État inconnu : {string.Join(", ", ProposalStatuses.All)}.");
    }
}
