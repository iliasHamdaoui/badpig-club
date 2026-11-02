using BadPig.Administration.Core;
using BadPig.Shared.Core;

namespace BadPig.Announcements.Core;

/// <summary>Decides, from the token and the membership, what the signed-in person (or the visitor) can read.</summary>
public sealed class AnnouncementViewer(IUserContext currentUser, IMembershipReader memberships, IDateTimeProvider clock)
{
    /// <summary>Today's date in Paris, to compare with the end date of the announcements.</summary>
    public DateOnly Today => AnnouncementExpiry.TodayAt(clock.UtcNow);

    public async Task<AnnouncementAudience> ResolveAsync(CancellationToken ct)
    {
        if (IsOnTheBoard)
            return AnnouncementAudience.Board;

        if (IsNotAMember)
            return AnnouncementAudience.Visitor;

        return await HasAnUpToDateMembershipAsync(ct) ? AnnouncementAudience.Member : AnnouncementAudience.Visitor;
    }

    private bool IsOnTheBoard => currentUser.IsInGroup(ClubGroup.Board) || currentUser.IsInGroup(ClubGroup.Admin);

    private bool IsNotAMember => currentUser.AccountId is null || !currentUser.IsInGroup(ClubGroup.Members);

    // Same rule as the members area of the site: an unvalidated membership (after the grace period) loses the members area.
    private async Task<bool> HasAnUpToDateMembershipAsync(CancellationToken ct) =>
        (await memberships.GetAsync(currentUser.AccountId!, ct)).State != MembershipStates.ToValidate;
}
