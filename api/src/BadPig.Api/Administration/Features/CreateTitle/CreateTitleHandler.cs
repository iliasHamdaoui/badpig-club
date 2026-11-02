using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.CreateTitle;

/// <summary>Creates a board title (unique name, case-insensitive).</summary>
internal sealed class CreateTitleHandler(IValidator<CreateTitleRequest> validator, IDateTimeProvider clock, 
    AppDbContext db, IAdministrationAudit auditLog) : IHandler<CreateTitleRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateTitleRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var label = TitleLabel.Normalize(request.Label!);

        var lower = label.ToLower();
        if (await db.Titles.AnyAsync(t => t.Label.ToLower() == lower, ct))
            return Error.Conflict("Ce titre existe déjà.");

        var title = new Title { Id = Guid.NewGuid(), Label = label, CreatedAt = clock.UtcNow };
        db.Titles.Add(title);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another request created the same name at the same time (unique index).
            return Error.Conflict("Ce titre existe déjà.");
        }

        await auditLog.RecordAsync($"title.created.{label}", null, ct);
        return new CreatedResponse(title.Id.ToString());
    }
}
