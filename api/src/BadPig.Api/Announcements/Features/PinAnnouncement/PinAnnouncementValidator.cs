using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.PinAnnouncement;

internal sealed class PinAnnouncementValidator : AbstractValidator<PinAnnouncementRequest>
{
    public PinAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
