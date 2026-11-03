using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Profile.Features.RemoveMyPhoto;

/// <summary>Removes the profile picture of the signed-in person (nothing happens when there is none).</summary>
internal sealed class RemoveMyPhotoHandler(IUserContext currentUser, AppDbContext db) : IHandler<RemoveMyPhotoRequest>
{
    public async Task<Result> HandleAsync(RemoveMyPhotoRequest request, CancellationToken ct)
    {
        var id = Guid.Parse(currentUser.AccountId!);
        await db.MemberPhotos.Where(p => p.AccountId == id).ExecuteDeleteAsync(ct);
        return Result.Success();
    }
}
