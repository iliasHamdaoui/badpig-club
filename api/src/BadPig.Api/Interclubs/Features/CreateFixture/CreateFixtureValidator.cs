using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.CreateFixture;

internal sealed class CreateFixtureValidator : AbstractValidator<CreateFixtureRequest>
{
    public CreateFixtureValidator()
    {
        RuleFor(x => x.TeamId).MustBeGuid();
        RuleFor(x => x.Body.Date).MustBeAFixtureDate();
        RuleFor(x => x.Body.Opponent).MustBeAnOpponent();
        RuleFor(x => x.Body.Venue).MustBeAVenue();
        RuleFor(x => x.Body.Place).MustBeAPlace();
    }
}
