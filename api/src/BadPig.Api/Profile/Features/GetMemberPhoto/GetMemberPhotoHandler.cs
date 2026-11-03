using BadPig.Profile.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Profile.Features.GetMemberPhoto;

/// <summary>The profile picture of a person, for the members with an up-to-date membership and the board.</summary>
internal sealed class GetMemberPhotoHandler(IValidator<GetMemberPhotoRequest> validator, PhotoAccess access, AppDbContext db)
    : IHandler<GetMemberPhotoRequest, GetMemberPhotoResponse>
{
    public async Task<Result<GetMemberPhotoResponse>> HandleAsync(GetMemberPhotoRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var photo = await db.MemberPhotos.AsNoTracking().FirstOrDefaultAsync(p => p.AccountId == id, ct);
        if (photo is null)
            return Error.NotFound();

        return new GetMemberPhotoResponse(photo.Content, photo.ContentType);
    }
}
