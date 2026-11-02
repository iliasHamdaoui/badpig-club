using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.SetProductImage;

/// <summary>Sets the picture of an article; the previous picture, if any, is replaced.</summary>
internal sealed class SetProductImageHandler(IValidator<SetProductImageRequest> validator, IDateTimeProvider clock, IShopAudit audit, AppDbContext db)
    : IHandler<SetProductImageRequest>
{
    public const string AuditAction = "product.image_set";

    public async Task<Result> HandleAsync(SetProductImageRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var product = await db.ShopProducts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return Error.NotFound();

        using var buffer = new MemoryStream();
        await request.File!.Content.CopyToAsync(buffer, ct);
        var content = buffer.ToArray();
        // The type comes from the first bytes, never from the name or the type announced by the sender.
        var contentType = ContentTypeOf(content);
        if (contentType is null)
            return Error.Validation("file", "Le fichier n'est pas une image valide (JPEG, PNG ou WebP).");

        var image = await db.ProductImages.FirstOrDefaultAsync(i => i.ProductId == id, ct);
        if (image is null)
            db.ProductImages.Add(new ProductImage { ProductId = id, ContentType = contentType, Content = content, UpdatedAt = clock.UtcNow });
        else
        {
            image.ContentType = contentType;
            image.Content = content;
            image.UpdatedAt = clock.UtcNow;
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Error.Conflict("L'article a été modifié en même temps : recommencez.");
        }

        await audit.RecordAsync(AuditAction, id, product.Name, null, ct);
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
