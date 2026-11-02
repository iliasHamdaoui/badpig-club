using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.GetAnnouncement;

internal sealed class GetAnnouncementValidator : AbstractValidator<GetAnnouncementRequest>
{
    public GetAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
