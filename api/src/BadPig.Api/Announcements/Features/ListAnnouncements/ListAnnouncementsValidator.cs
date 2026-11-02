using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.ListAnnouncements;

internal sealed class ListAnnouncementsValidator : ListQueryValidator<ListAnnouncementsRequest>
{
    public ListAnnouncementsValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("publishedAt", "createdAt", "title");
        RuleFor(x => x.Kind).Must(kind => string.IsNullOrEmpty(kind) || AnnouncementKinds.IsValid(kind))
            .WithMessage("Le type est « internal » ou « public ».");
        RuleFor(x => x.Form).Must(form => string.IsNullOrEmpty(form) || form is AnnouncementForms.Text or AnnouncementForms.Poll)
            .WithMessage("La forme est « text » ou « poll ».");
        RuleFor(x => x.Status).Must(status => string.IsNullOrEmpty(status) || status is "published" or "draft")
            .WithMessage("Le statut est « published » ou « draft ».");
    }
}
