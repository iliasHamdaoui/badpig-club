using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.UnpinAnnouncement;

internal sealed class UnpinAnnouncementValidator : AbstractValidator<UnpinAnnouncementRequest>
{
    public UnpinAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
