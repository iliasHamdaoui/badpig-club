using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.UnlikeAnnouncement;

internal sealed class UnlikeAnnouncementValidator : AbstractValidator<UnlikeAnnouncementRequest>
{
    public UnlikeAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
