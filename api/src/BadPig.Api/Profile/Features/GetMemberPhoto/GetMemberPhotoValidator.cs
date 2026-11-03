using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Profile.Features.GetMemberPhoto;

internal sealed class GetMemberPhotoValidator : AbstractValidator<GetMemberPhotoRequest>
{
    public GetMemberPhotoValidator() => RuleFor(x => x.Id).MustBeGuid();
}
