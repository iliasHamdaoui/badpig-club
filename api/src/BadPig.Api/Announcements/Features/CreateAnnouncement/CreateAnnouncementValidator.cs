using BadPig.Announcements.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.CreateAnnouncement;

internal sealed class CreateAnnouncementValidator : AbstractValidator<CreateAnnouncementRequest>
{
    public CreateAnnouncementValidator()
    {
        RuleFor(x => x.Title).MustBeAnAnnouncementTitle();
        RuleFor(x => x.BodyHtml).MustBeAnAnnouncementBody();
        RuleFor(x => x.Kind).MustBeAnAnnouncementKind();
    }
}
