using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.LikeAnnouncement;

internal sealed class LikeAnnouncementValidator : AbstractValidator<LikeAnnouncementRequest>
{
    public LikeAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
