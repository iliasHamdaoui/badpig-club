using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.PublishAnnouncement;

internal sealed class PublishAnnouncementValidator : AbstractValidator<PublishAnnouncementRequest>
{
    public PublishAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
