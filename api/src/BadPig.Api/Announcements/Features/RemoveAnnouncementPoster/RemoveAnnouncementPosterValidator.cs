using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.RemoveAnnouncementPoster;

internal sealed class RemoveAnnouncementPosterValidator : AbstractValidator<RemoveAnnouncementPosterRequest>
{
    public RemoveAnnouncementPosterValidator() => RuleFor(x => x.Id).MustBeGuid();
}
