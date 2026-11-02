using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListBoardMembers;

/// <summary>
/// Who is on the board, with the way to write to them: the club members need it to know who to ask.
/// Only the active accounts of the board group are listed; the names and e-mails come from the identity provider.
/// </summary>
internal sealed class ListBoardMembersHandler(IValidator<ListBoardMembersRequest> validator, IIdentityProvider identity, AppDbContext db)
    : IHandler<ListBoardMembersRequest, PagedResponse<ListBoardMembersItem>>
{
    public async Task<Result<PagedResponse<ListBoardMembersItem>>> HandleAsync(ListBoardMembersRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var titles = (await db.BoardTitles.AsNoTracking().Include(b => b.Title).ToListAsync(ct))
            .Where(b => b.Title is not null)
            .ToDictionary(b => b.MemberId.ToString(), b => b.Title!.Label);

        var items = (await identity.ListAsync(ct))
            .Where(a => a.IsActive && a.IsInGroup(ClubGroup.Board))
            .Select(a => new ListBoardMembersItem(a.Id, a.Name, a.Email, titles.GetValueOrDefault(a.Id)))
            .ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Name.HasText(term) || i.Title.HasText(term)).ToList();

        var descending = request.IsDescending();
        // By title: the people who hold one first (false sorts before true), then the others, each group by name.
        var sorted = request.Sort == "title"
            ? items.SortBy(i => i.Title is null, descending).ThenSort(i => i.Title ?? "", AlphabeticalComparer.Instance).ThenSort(i => i.Name, AlphabeticalComparer.Instance)
            : items.SortBy(i => i.Name, descending, AlphabeticalComparer.Instance);
        return sorted.ToPage(request);
    }
}
