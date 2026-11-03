using BadPig.Profile.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Profile.Features.SetMyPhoto;

/// <summary>Sets the profile picture of the signed-in person; the previous picture, if any, is replaced.</summary>
internal sealed class SetMyPhotoHandler(IValidator<SetMyPhotoRequest> validator, IUserContext currentUser, IDateTimeProvider clock, AppDbContext db)
    : IHandler<SetMyPhotoRequest>
{
    public async Task<Result> HandleAsync(SetMyPhotoRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        using var buffer = new MemoryStream();
        await request.File!.Content.CopyToAsync(buffer, ct);
        var content = buffer.ToArray();
        // The type comes from the first bytes, never from the name or the type announced by the sender.
        var contentType = ContentTypeOf(content);
        if (contentType is null)
            return Error.Validation("file", "Le fichier n'est pas une image valide (JPEG, PNG ou WebP).");

        var id = Guid.Parse(currentUser.AccountId!);
        var photo = await db.MemberPhotos.FirstOrDefaultAsync(p => p.AccountId == id, ct);
        if (photo is null)
            db.MemberPhotos.Add(new MemberPhoto { AccountId = id, ContentType = contentType, Content = content, UpdatedAt = clock.UtcNow });
        else
        {
            photo.ContentType = contentType;
            photo.Content = content;
            photo.UpdatedAt = clock.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Error.Conflict("Votre photo a été modifiée en même temps : recommencez.");
        }

        return Result.Success();
    }

    private static string? ContentTypeOf(ReadOnlySpan<byte> header)
    {
        if (header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";
        if (header.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }))
            return "image/jpeg";
        if (header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }
}
