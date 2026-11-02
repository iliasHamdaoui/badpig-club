using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.UpdateFixture;

internal sealed class UpdateFixtureValidator : AbstractValidator<UpdateFixtureRequest>
{
    public UpdateFixtureValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Date).MustBeAFixtureDate();
        RuleFor(x => x.Body.Opponent).MustBeAnOpponent();
        RuleFor(x => x.Body.Venue).MustBeAVenue();
        RuleFor(x => x.Body.Place).MustBeAPlace();
    }
}
