using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.UpdateAnnouncement;

internal sealed class UpdateAnnouncementValidator : AbstractValidator<UpdateAnnouncementRequest>
{
    public UpdateAnnouncementValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Title).MustBeAnAnnouncementTitle();
        RuleFor(x => x.Body.BodyHtml).Must(body => body is null || body.Length <= AnnouncementValidationRules.MaxBodyLength)
            .WithMessage($"Le contenu est trop long ({AnnouncementValidationRules.MaxBodyLength} caractères au plus).");
        RuleFor(x => x.Body.Kind).MustBeAnAnnouncementKind();
    }
}
