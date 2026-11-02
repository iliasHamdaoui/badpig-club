using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.ListStockMovements;

/// <summary>The stock history of a variant, most recent first by default.</summary>
internal sealed class ListStockMovementsHandler(IValidator<ListStockMovementsRequest> validator, AppDbContext db)
    : IHandler<ListStockMovementsRequest, PagedResponse<ListStockMovementsItem>>
{
    public async Task<Result<PagedResponse<ListStockMovementsItem>>> HandleAsync(ListStockMovementsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        if (!await db.ProductVariants.AnyAsync(v => v.Id == id, ct))
            return Error.NotFound();

        var query = db.StockMovements.AsNoTracking().Where(m => m.VariantId == id);
        if (request.SearchTerm() is { } term)
            query = query.Where(m => m.Note.ToLower().Contains(term) || m.AuthorName.ToLower().Contains(term));

        var ordered = request.IsDescending(defaultDescending: true) ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt);
        var page = await ordered.ThenBy(m => m.Id).ToPageAsync(request, ct);
        return page.Map(m => new ListStockMovementsItem(m.Id.ToString(), m.Change, m.Reason, m.Note, m.AuthorName, m.OrderId?.ToString(), m.CreatedAt));
    }
}
