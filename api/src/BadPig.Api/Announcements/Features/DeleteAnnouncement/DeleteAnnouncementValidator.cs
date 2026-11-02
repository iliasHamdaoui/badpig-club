using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.DeleteAnnouncement;

internal sealed class DeleteAnnouncementValidator : AbstractValidator<DeleteAnnouncementRequest>
{
    public DeleteAnnouncementValidator() => RuleFor(x => x.Id).MustBeGuid();
}
