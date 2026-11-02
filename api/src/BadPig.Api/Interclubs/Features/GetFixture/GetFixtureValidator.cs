using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.GetFixture;

internal sealed class GetFixtureValidator : AbstractValidator<GetFixtureRequest>
{
    public GetFixtureValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
