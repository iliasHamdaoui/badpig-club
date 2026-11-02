using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.AddAnnouncementAttachment;

internal sealed class AddAnnouncementAttachmentValidator : AbstractValidator<AddAnnouncementAttachmentRequest>
{
    public AddAnnouncementAttachmentValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.File).MustBeAnAttachment();
    }
}
