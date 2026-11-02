using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.GetAnnouncementFile;

internal sealed class GetAnnouncementFileValidator : AbstractValidator<GetAnnouncementFileRequest>
{
    public GetAnnouncementFileValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.FileId).MustBeGuid();
    }
}
