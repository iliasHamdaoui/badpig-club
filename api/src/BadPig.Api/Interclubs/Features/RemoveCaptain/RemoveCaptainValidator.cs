using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.RemoveCaptain;

internal sealed class RemoveCaptainValidator : AbstractValidator<RemoveCaptainRequest>
{
    public RemoveCaptainValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
