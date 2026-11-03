using BadPig.Profile.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Profile.Features.ListClubDirectory;

/// <summary>
/// The club directory (« trombinoscope »): the active members who chose to put a profile picture. Someone who has not added a picture
/// does not appear: the club does not show the name of a person who did not choose to be shown. The names come from the identity provider.
/// </summary>
internal sealed class ListClubDirectoryHandler(IValidator<ListClubDirectoryRequest> validator, PhotoAccess access, IIdentityProvider identity, AppDbContext db)
    : IHandler<ListClubDirectoryRequest, PagedResponse<ListClubDirectoryItem>>
{
    public async Task<Result<PagedResponse<ListClubDirectoryItem>>> HandleAsync(ListClubDirectoryRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var rows = await db.MemberPhotos.AsNoTracking().Select(p => new { p.AccountId, p.UpdatedAt }).ToListAsync(ct);
        var versions = rows.ToDictionary(r => r.AccountId.ToString(), r => r.UpdatedAt.ToUnixTimeMilliseconds());

        bool IsInDirectory(AccountInfo account) => account.IsActive && AccountRules.IsClubMember(account) && versions.ContainsKey(account.Id);

        var items = (await identity.ListAsync(ct))
            .Where(IsInDirectory)
            .Select(a => new ListClubDirectoryItem(a.Id, a.Name, versions[a.Id]))
            .ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Name.HasText(term)).ToList();

        return items.SortBy(i => i.Name, request.IsDescending(), AlphabeticalComparer.Instance).ToPage(request);
    }
}
