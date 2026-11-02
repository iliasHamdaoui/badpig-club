using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.DeleteSeason;

internal sealed class DeleteSeasonValidator : AbstractValidator<DeleteSeasonRequest>
{
    public DeleteSeasonValidator() => RuleFor(x => x.Id).MustBeGuid();
}
