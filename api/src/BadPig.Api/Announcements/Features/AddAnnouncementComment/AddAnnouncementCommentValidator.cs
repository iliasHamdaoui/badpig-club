using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.AddAnnouncementComment;

internal sealed class AddAnnouncementCommentValidator : AbstractValidator<AddAnnouncementCommentRequest>
{
    public AddAnnouncementCommentValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Text).NotEmpty().WithMessage("Le commentaire est vide.")
            .Must(text => text is null || text.Trim().Length <= CommentRules.MaxLength)
            .WithMessage($"Le commentaire est trop long ({CommentRules.MaxLength} caractères au plus).");
        RuleFor(x => x.Body.ParentId).Must(parent => string.IsNullOrEmpty(parent) || Guid.TryParse(parent, out _))
            .WithMessage("L'identifiant du commentaire est invalide.");
    }
}
