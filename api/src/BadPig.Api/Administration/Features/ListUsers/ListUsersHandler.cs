using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListUsers;

/// <summary>
/// Club people with what the signed-in person can do on each (rules: never on one's own account; a regular member can be
/// managed by the board; board and admin accounts only by an Admin). Technical accounts of the identity provider stay invisible.
/// </summary>
internal sealed class ListUsersHandler(IValidator<ListUsersRequest> validator, IDateTimeProvider clock, 
    IUserContext currentUser, IIdentityProvider identity, AppDbContext db)
    : IHandler<ListUsersRequest, PagedResponse<ListUsersItem>>
{
    public async Task<Result<PagedResponse<ListUsersItem>>> HandleAsync(ListUsersRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var sub = currentUser.AccountId;
        var isAdmin = currentUser.IsInGroup(ClubGroup.Admin);
        var accounts = (await identity.ListAsync(ct)).Where(AccountRules.IsClubMember).ToList();
        var invitations = (await db.Invitations.AsNoTracking().ToListAsync(ct)).ToDictionary(i => i.TargetId.ToString());
        var disabled = (await db.DisabledAccounts.AsNoTracking().ToListAsync(ct)).ToDictionary(d => d.AccountId.ToString(), d => d.DisabledSince);
        var seasons = await db.Seasons.AsNoTracking().ToListAsync(ct);
        var today = SeasonRules.Today(clock.UtcNow);
        var memberships = (await db.Memberships.AsNoTracking().ToListAsync(ct))
            .GroupBy(m => m.MemberId)
            .ToDictionary(g => g.Key.ToString(), g => (IReadOnlyCollection<Membership>)g.ToList());
        var licences = (await db.MemberLicences.AsNoTracking().ToListAsync(ct)).ToDictionary(l => l.AccountId.ToString(), l => l.Number);
        var titles = (await db.BoardTitles.AsNoTracking().Include(b => b.Title).ToListAsync(ct))
            .Where(b => b.Title is not null)
            .ToDictionary(b => b.MemberId.ToString(), b => b.Title!);
        var boardCount = accounts.Count(a => a.IsActive && a.IsInGroup(ClubGroup.Board));

        var rows = accounts.Select(a =>
        {
            invitations.TryGetValue(a.Id, out var invitation);
            var membership = MembershipStatus.Of(seasons, memberships.GetValueOrDefault(a.Id) ?? [], today);
            titles.TryGetValue(a.Id, out var title);
            // Waiting for signup = the person has never signed in; the invitation line only says who invited them and when
            // (it may be missing for an account created straight in the identity provider).
            var status = !a.IsActive ? AccountStatuses.Disabled
                : a.LastLogin is null ? AccountStatuses.Pending
                : AccountStatuses.Active;
            return new ListUsersItem(
                a.Id, a.Username, a.Name, a.Email, a.IsActive, AccountRules.ClubGroups(a), AccountRules.CanManage(a, sub, isAdmin),
                status, invitation?.InvitedByName, invitation?.CreatedAt,
                BoardRules.AllowedAction(a, sub, isAdmin, boardCount),
                membership.State, membership.SeasonName, membership.Kind ?? invitation?.MembershipKind ?? MembershipKinds.Internal, membership.Amount,
                membership.ValidatedAt, membership.ValidatedByName,
                a.IsActive && a.Id != sub, licences.GetValueOrDefault(a.Id), title?.Id.ToString(), title?.Label,
                !a.IsActive && disabled.TryGetValue(a.Id, out var since) && !AccountRules.IsBoardOrAdmin(a)
                    ? DisabledAccount.DeletionDate(since) : null);
        });

        if (request.SearchTerm() is { } term)
            rows = rows.Where(r => r.Name.HasText(term) || r.Email.HasText(term) || r.Username.HasText(term) || r.Licence.HasText(term));

        var desc = request.IsDescending();
        var text = AlphabeticalComparer.Instance;
        IOrderedEnumerable<ListUsersItem>? sorted = request.Sort?.ToLowerInvariant() switch
        {
            null or "" or "name" => rows.SortBy(r => r.Name, desc, text),
            "email" => rows.SortBy(r => r.Email, desc, text),
            "status" => rows.SortBy(r => r.Status, desc, text),
            "membership" => rows.SortBy(r => r.Membership, desc, text),
            _ => null,
        };
        if (sorted is null)
            return Error.Validation("sort", "Tri invalide.");

        return sorted.ThenSort(r => r.Name, text).ToPage(request);
    }
}
