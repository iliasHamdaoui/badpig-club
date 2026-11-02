using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.AssignCaptain;

internal sealed class AssignCaptainValidator : AbstractValidator<AssignCaptainRequest>
{
    public AssignCaptainValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.CaptainId).MustBeGuid();
    }
}
