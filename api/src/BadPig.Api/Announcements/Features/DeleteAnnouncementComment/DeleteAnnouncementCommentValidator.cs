using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.DeleteAnnouncementComment;

internal sealed class DeleteAnnouncementCommentValidator : AbstractValidator<DeleteAnnouncementCommentRequest>
{
    public DeleteAnnouncementCommentValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.CommentId).MustBeGuid();
    }
}
