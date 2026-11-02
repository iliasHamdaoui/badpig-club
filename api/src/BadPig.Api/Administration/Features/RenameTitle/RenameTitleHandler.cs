using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.RenameTitle;

/// <summary>Renames a board title (unique name, case-insensitive).</summary>
internal sealed class RenameTitleHandler(IValidator<RenameTitleRequest> validator, 
    AppDbContext db, IAdministrationAudit auditLog) : IHandler<RenameTitleRequest>
{
    public async Task<Result> HandleAsync(RenameTitleRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var label = TitleLabel.Normalize(request.Label!);

        var title = Guid.TryParse(request.Id, out var titleId) ? await db.Titles.FirstOrDefaultAsync(t => t.Id == titleId, ct) : null;
        if (title is null)
            return Error.NotFound();

        var lower = label.ToLower();
        if (await db.Titles.AnyAsync(t => t.Id != titleId && t.Label.ToLower() == lower, ct))
            return Error.Conflict("Ce titre existe déjà.");

        var oldLabel = title.Label;
        title.Label = label;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Error.Conflict("Ce titre existe déjà.");
        }

        if (oldLabel != label)
            await auditLog.RecordAsync($"title.renamed.{oldLabel} → {label}", null, ct);

        return Result.Success();
    }
}
