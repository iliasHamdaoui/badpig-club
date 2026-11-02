using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.CreateDateProposal;

internal sealed class CreateDateProposalValidator : AbstractValidator<CreateDateProposalRequest>
{
    public CreateDateProposalValidator()
    {
        RuleFor(x => x.TeamId).MustBeGuid();
        RuleFor(x => x.Body.Opponent).MustBeAnOpponent();
        RuleFor(x => x.Body.Venue).MustBeAVenue();
        RuleFor(x => x.Body.Place).MustBeAPlace();
        RuleFor(x => x.Body.Dates).Must(HaveBetweenTwoAndTenDifferentDates)
            .WithMessage("Proposez entre 2 et 10 dates, toutes différentes.");
    }

    private const int MinDates = 2;
    private const int MaxDates = 10;

    private static bool HaveBetweenTwoAndTenDifferentDates(List<CreateDateProposalDate>? dates) =>
        dates is { Count: >= MinDates and <= MaxDates }
        && dates.All(d => d.Date is not null)
        && dates.Select(d => (d.Date, d.StartTime)).Distinct().Count() == dates.Count;
}
