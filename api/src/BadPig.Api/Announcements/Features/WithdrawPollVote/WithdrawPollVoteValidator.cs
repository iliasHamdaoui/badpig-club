using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.WithdrawPollVote;

internal sealed class WithdrawPollVoteValidator : AbstractValidator<WithdrawPollVoteRequest>
{
    public WithdrawPollVoteValidator() => RuleFor(x => x.Id).MustBeGuid();
}
