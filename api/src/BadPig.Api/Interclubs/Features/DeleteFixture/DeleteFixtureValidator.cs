using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.DeleteFixture;

internal sealed class DeleteFixtureValidator : AbstractValidator<DeleteFixtureRequest>
{
    public DeleteFixtureValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
