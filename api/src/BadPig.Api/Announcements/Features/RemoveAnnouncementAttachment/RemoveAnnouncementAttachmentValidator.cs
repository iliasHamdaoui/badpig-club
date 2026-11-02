using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.RemoveAnnouncementAttachment;

internal sealed class RemoveAnnouncementAttachmentValidator : AbstractValidator<RemoveAnnouncementAttachmentRequest>
{
    public RemoveAnnouncementAttachmentValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.FileId).MustBeGuid();
    }
}
