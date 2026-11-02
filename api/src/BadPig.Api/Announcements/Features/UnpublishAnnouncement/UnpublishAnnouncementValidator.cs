using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.UnpublishAnnouncement;

internal sealed class UnpublishAnnouncementValidator : AbstractValidator<UnpublishAnnouncementRequest>
{
    public UnpublishAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
