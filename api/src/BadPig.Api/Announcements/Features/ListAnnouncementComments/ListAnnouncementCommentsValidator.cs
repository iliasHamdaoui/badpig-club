using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.ListAnnouncementComments;

internal sealed class ListAnnouncementCommentsValidator : ListQueryValidator<ListAnnouncementCommentsRequest>
{
    public ListAnnouncementCommentsValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Sort).MustBeSortableOn("createdAt");
    }
}
