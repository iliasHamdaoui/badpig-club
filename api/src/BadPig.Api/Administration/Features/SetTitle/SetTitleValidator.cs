using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.SetTitle;

internal sealed class SetTitleValidator : AbstractValidator<SetTitleRequest>
{
    public SetTitleValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        // Empty or null removes the person's title.
        RuleFor(x => x.TitleId).Must(id => string.IsNullOrWhiteSpace(id) || Guid.TryParse(id, out _)).WithMessage("Identifiant de titre invalide.");
    }
}
