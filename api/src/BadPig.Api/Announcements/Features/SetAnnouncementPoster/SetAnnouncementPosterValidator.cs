using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.SetAnnouncementPoster;

internal sealed class SetAnnouncementPosterValidator : AbstractValidator<SetAnnouncementPosterRequest>
{
    public SetAnnouncementPosterValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.File).MustBeAPoster();
    }
}
